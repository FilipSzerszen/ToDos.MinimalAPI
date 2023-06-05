﻿using FluentValidation;

namespace ToDos.MinimalAPI;

public class ToDoValidator : AbstractValidator<ToDo>
{
    public ToDoValidator()
    {
        RuleFor(r => r.Value)
            .NotEmpty()
            .MinimumLength(5)
            .WithMessage("Value of ToDo must be atleast 5 characters");
    }
}

