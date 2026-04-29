using CSharpFunctionalExtensions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Validation;

public static class CustomValidators
{
    public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TValueObject, Errors>> factoryMethod)
    {
        return ruleBuilder.Custom((element, context) =>
        {
            var result = factoryMethod(element);
            if (result.IsSuccess)
                return;

            context.AddFailure(result.Error.Serialize());
        });
    }

    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> ruleBuilder, Error error
    )
    {
        return ruleBuilder.WithMessage(error.Serialize());
    }

    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> ruleBuilder, Errors errors
    )
    {
        return ruleBuilder.WithMessage(errors.Serialize());
    }
}