using FluentValidation;

using Microsoft.AspNetCore.Authorization;

namespace ToDos.MinimalAPI;
public static class ToDoRequests
{
    public static WebApplication RegisterEndpoints(this WebApplication app)
    {
        app.MapGet("/todos", ToDoRequests.GetAll)
            .Produces<List<ToDo>>()
            .WithTags("To Dos")
            .RequireAuthorization();                    // I opcja autentykacji minimal web api

        app.MapGet("/todos/{id}", ToDoRequests.GetById)
            .Produces<ToDo>()
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("To Dos");

        app.MapPost("/todos", ToDoRequests.Create)
            .Produces<ToDo>(StatusCodes.Status201Created)
            .Accepts<ToDo>("application/json")
            .WithTags("To Dos")
            .WithValidator<ToDo>();         // można zwalidować modelmetodą rozszerzoną albo w środku metody np update

        app.MapPut("/todos/{id}", ToDoRequests.Update)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status204NoContent)
            .Accepts<ToDo>("application/json")
            .WithTags("To Dos");

        app.MapDelete("/todos/{id}", ToDoRequests.Delete)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status204NoContent)
            .WithTags("To Dos")
            .ExcludeFromDescription();          // ukrywa defiinicję endpointu w swagerze

        return app;
    }

    public static IResult GetAll(IToDoService service)
    {
        var todos = service.GetAll();
        return Results.Ok(todos);
    }
    public static IResult GetById(IToDoService service, Guid id)
    {
        var todo = service.GetById(id);
        if (todo == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(todo);
    }

    [Authorize]                 //II druga opcja autentykacji minimal web api
    public static IResult Create(IToDoService service, ToDo toDo)
    {
        service.Create(toDo);
        return Results.Created($"/todos/{toDo.Id}", toDo);
    }
    public static IResult Update(IToDoService service, ToDo toDo, Guid id, IValidator<ToDo> validator)
    {
        var validationResult = validator.Validate(toDo);

        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var todo = service.GetById(id);
        if (todo == null)
        {
            return Results.NotFound();
        }
        service.Update(todo);
        return Results.NoContent();
    }
    public static IResult Delete(IToDoService service, Guid id)
    {
        var todo = service.GetById(id);
        if (todo == null)
        {
            return Results.NotFound();
        }
        service.Delete(id);
        return Results.NoContent();
    }
}

