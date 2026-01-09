using Agent.Api.DTOs;
using Agent.Core.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Agent.Api.Endpoints;

public static class ConversationEndpoints
{
	public static IEndpointRouteBuilder MapConversations(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/conversations")
			.WithTags("Conversations");

		group.MapGet("/", GetAllConversationsAsync)
			.WithName("GetConversations")
			.WithSummary("Get all conversation threads")
			.Produces<List<ConversationDto>>(StatusCodes.Status200OK);

		group.MapGet("/{id:guid}", GetConversationByIdAsync)
			.WithName("GetConversationById")
			.WithSummary("Get a conversation thread with its messages")
			.Produces<ConversationDetailDto>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/{id:guid}", DeleteConversationAsync)
			.WithName("DeleteConversation")
			.WithSummary("Delete a conversation thread and all its messages")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		return endpoints;
	}

	private static async Task<IResult> GetAllConversationsAsync(
		IDbContextFactory<ChatDbContext> dbContextFactory,
		int page = 1,
		int pageSize = 20,
		CancellationToken cancellationToken = default)
	{
		await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		var skip = (page - 1) * pageSize;

		var conversations = await dbContext.ChatThreads
			.AsNoTracking()
			.OrderByDescending(t => t.UpdatedAt)
			.Skip(skip)
			.Take(pageSize)
			.Select(t => new ConversationDto
			{
				Id = t.Id,
				ThreadId = t.ThreadId,
				Title = t.Title,
				CreatedAt = t.CreatedAt,
				UpdatedAt = t.UpdatedAt,
				UserId = t.UserId
			})
			.ToListAsync(cancellationToken);

		var totalCount = await dbContext.ChatThreads.CountAsync(cancellationToken);

		return Results.Ok(new PagedResult<ConversationDto>
		{
			Items = conversations,
			TotalCount = totalCount,
			Page = page,
			PageSize = pageSize,
			TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
		});
	}

	private static async Task<IResult> GetConversationByIdAsync(
		Guid id,
		IDbContextFactory<ChatDbContext> dbContextFactory,
		CancellationToken cancellationToken = default)
	{
		await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		var thread = await dbContext.ChatThreads
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

		if (thread is null)
		{
			return Results.NotFound(new { message = $"Conversation with ID '{id}' not found." });
		}

		// Get all messages for this thread
		var messages = await dbContext.ChatMessages
			.AsNoTracking()
			.Where(m => m.ThreadId == thread.ThreadId)
			.OrderBy(m => m.SequenceNumber)
			.Select(m => new MessageDto
			{
				Id = m.Id,
				MessageId = m.MessageId,
				Role = m.Role,
				Content = m.Content,
				CreatedAt = m.CreatedAt,
				SequenceNumber = m.SequenceNumber
			})
			.ToListAsync(cancellationToken);

		var result = new ConversationDetailDto
		{
			Id = thread.Id,
			ThreadId = thread.ThreadId,
			Title = thread.Title,
			CreatedAt = thread.CreatedAt,
			UpdatedAt = thread.UpdatedAt,
			UserId = thread.UserId,
			Messages = messages,
			MessageCount = messages.Count
		};

		return Results.Ok(result);
	}

	private static async Task<IResult> DeleteConversationAsync(
		Guid id,
		IDbContextFactory<ChatDbContext> dbContextFactory,
		CancellationToken cancellationToken = default)
	{
		await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		var thread = await dbContext.ChatThreads
			.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

		if (thread is null)
		{
			return Results.NotFound(new { message = $"Conversation with ID '{id}' not found." });
		}

		// Delete all messages first (cascade)
		await dbContext.ChatMessages
			.Where(m => m.ThreadId == thread.ThreadId)
			.ExecuteDeleteAsync(cancellationToken);

		// Delete the thread
		dbContext.ChatThreads.Remove(thread);
		await dbContext.SaveChangesAsync(cancellationToken);

		return Results.NoContent();
	}
}
