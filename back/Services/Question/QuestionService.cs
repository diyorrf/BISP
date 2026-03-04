using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using back.Data.Repos.Interfaces;
using back.Models.DTOs;
using back.Services.AI;
using QuestionEntity = back.Data.Entities.Question;

namespace back.Services.Question
{
    public class QuestionService: IQuestionService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAIService _aiService;
        private readonly ILogger<QuestionService> _logger;

        public QuestionService(
            IDocumentRepository documentRepository,
            IQuestionRepository questionRepository,
            IUserRepository userRepository,
            IAIService aiService,
            ILogger<QuestionService> logger)
        {
            _documentRepository = documentRepository;
            _questionRepository = questionRepository;
            _userRepository = userRepository;
            _aiService = aiService;
            _logger = logger;
        }

        public async Task<QuestionResponseDto> AskQuestionAsync(QuestionRequestDto request, long userId, CancellationToken ct = default)
        {
            var document = await _documentRepository.GetByIdAndUserIdAsync(request.DocumentId, userId, ct);
            
            if (document == null)
                throw new ArgumentException($"Document {request.DocumentId} not found");

            var question = new QuestionEntity
            {
                Id = Guid.NewGuid(),
                DocumentId = request.DocumentId,
                UserId = userId,
                QuestionText = request.QuestionText,
                AskedAt = DateTime.UtcNow
            };

            var sw = Stopwatch.StartNew();
            
            try
            {
                var answer = await _aiService.GetAnswerAsync(document.Content, request.QuestionText, ct);
                
                sw.Stop();

                question.Answer = answer;
                question.AnsweredAt = DateTime.UtcNow;
                question.ProcessingTime = sw.Elapsed;
                question.TokensUsed = EstimateTokens(document.Content + request.QuestionText + answer);

                await _questionRepository.AddAsync(question, ct);

                await DecrementUserTokensAsync(userId, question.TokensUsed);

                _logger.LogInformation("Question {QuestionId} answered in {Duration}ms", 
                    question.Id, sw.ElapsedMilliseconds);

                return new QuestionResponseDto(
                    question.Id,
                    answer,
                    question.TokensUsed,
                    question.ProcessingTime
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing question {QuestionId}", question.Id);
                throw;
            }
        }

        public async IAsyncEnumerable<StreamChunkDto> AskQuestionStreamAsync(
            QuestionRequestDto request, 
            long userId,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var document = await _documentRepository.GetByIdAndUserIdAsync(request.DocumentId, userId, ct);
            
            if (document == null)
                throw new ArgumentException($"Document {request.DocumentId} not found");

            var question = new QuestionEntity
            {
                Id = Guid.NewGuid(),
                DocumentId = request.DocumentId,
                UserId = userId,
                QuestionText = request.QuestionText,
                AskedAt = DateTime.UtcNow
            };

            var sw = Stopwatch.StartNew();
            var fullAnswer = new StringBuilder();

            await foreach (var chunk in _aiService.GetAnswerStreamAsync(document.Content, request.QuestionText, ct))
            {
                fullAnswer.Append(chunk);
                yield return new StreamChunkDto(chunk, false);
            }

            sw.Stop();

            question.Answer = fullAnswer.ToString();
            question.AnsweredAt = DateTime.UtcNow;
            question.ProcessingTime = sw.Elapsed;
            question.TokensUsed = EstimateTokens(document.Content + request.QuestionText + question.Answer);

            await _questionRepository.AddAsync(question, ct);

            await DecrementUserTokensAsync(userId, question.TokensUsed);

            _logger.LogInformation("Streaming question {QuestionId} completed in {Duration}ms", 
                question.Id, sw.ElapsedMilliseconds);

            yield return new StreamChunkDto(string.Empty, true);
        }

        private async Task DecrementUserTokensAsync(long userId, int tokensUsed)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.TokensRemaining = Math.Max(0, user.TokensRemaining - tokensUsed);
                await _userRepository.UpdateAsync(user);
            }
        }

        private static int EstimateTokens(string text)
        {
            // Rough estimation: ~4 characters per token
            return text.Length / 4;
        }
    }
}