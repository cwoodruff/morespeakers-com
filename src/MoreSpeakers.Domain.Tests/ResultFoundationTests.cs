using System.Reflection;

using FluentAssertions;

using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Domain.Tests;

public class ResultFoundationTests
{
    private const string ResultTypeName = "MoreSpeakers.Domain.Result";
    private const string GenericResultTypeName = "MoreSpeakers.Domain.Result`1";
    private const string ErrorTypeName = "MoreSpeakers.Domain.Error";
    private static readonly Assembly DomainAssembly = typeof(User).Assembly;

    [Fact]
    public void Foundation_contract_types_should_be_public_and_rooted_in_domain_namespace()
    {
        var resultType = GetResultType();
        var genericResultType = GetGenericResultType();
        var errorType = GetErrorType();

        resultType.IsPublic.Should().BeTrue();
        resultType.IsValueType.Should().BeTrue();
        resultType.Namespace.Should().Be("MoreSpeakers.Domain");

        genericResultType.IsPublic.Should().BeTrue();
        genericResultType.IsValueType.Should().BeTrue();
        genericResultType.IsGenericTypeDefinition.Should().BeTrue();
        genericResultType.GetGenericArguments().Should().ContainSingle();
        genericResultType.Namespace.Should().Be("MoreSpeakers.Domain");

        errorType.IsPublic.Should().BeTrue();
        errorType.Namespace.Should().Be("MoreSpeakers.Domain");
    }

    [Fact]
    public void Error_should_expose_code_message_and_exception_context()
    {
        var exception = new InvalidOperationException("boom");
        var error = CreateError("result.failure", "Something went wrong.", exception);

        GetPropertyValue<string>(error, "Code").Should().Be("result.failure");
        GetPropertyValue<string>(error, "Message").Should().Be("Something went wrong.");
        GetExceptionProperty(error.GetType()).GetValue(error).Should().BeSameAs(exception);
    }

    [Fact]
    public void Error_should_use_value_equality()
    {
        var first = CreateError("duplicate", "Already exists.", null);
        var second = CreateError("duplicate", "Already exists.", null);
        var third = CreateError("unexpected", "Something else failed.", null);

        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());
        first.Should().NotBe(third);
    }

    [Fact]
    public void Result_public_api_should_match_the_foundation_contract()
    {
        var resultType = GetResultType();
        var genericResultType = GetGenericResultType();
        var errorType = GetErrorType();

        GetRequiredProperty(resultType, "IsSuccess").PropertyType.Should().Be(typeof(bool));
        GetRequiredProperty(resultType, "IsFailure").PropertyType.Should().Be(typeof(bool));
        GetRequiredProperty(resultType, "Error").PropertyType.Should().Be(errorType);

        GetRequiredProperty(genericResultType.MakeGenericType(typeof(string)), "IsSuccess").PropertyType.Should().Be(typeof(bool));
        GetRequiredProperty(genericResultType.MakeGenericType(typeof(string)), "IsFailure").PropertyType.Should().Be(typeof(bool));
        GetRequiredProperty(genericResultType.MakeGenericType(typeof(string)), "Error").PropertyType.Should().Be(errorType);
        GetRequiredProperty(genericResultType.MakeGenericType(typeof(string)), "Value").PropertyType.Should().Be(typeof(string));

        GetStaticMethod(resultType, "Success", false, Type.EmptyTypes).ReturnType.Should().Be(resultType);
        GetGenericFactoryMethod(resultType, "Success", parameter => parameter.ParameterType.IsGenericParameter)
            .MakeGenericMethod(typeof(string))
            .ReturnType.Should().Be(genericResultType.MakeGenericType(typeof(string)));
        GetStaticMethod(resultType, "Failure", false, errorType).ReturnType.Should().Be(resultType);
        GetGenericFactoryMethod(resultType, "Failure", parameter => parameter.ParameterType == errorType)
            .MakeGenericMethod(typeof(string))
            .ReturnType.Should().Be(genericResultType.MakeGenericType(typeof(string)));
    }

    [Fact]
    public void Result_success_factory_should_create_a_successful_void_result()
    {
        var result = InvokeSuccessFactory();

        GetPropertyValue<bool>(result, "IsSuccess").Should().BeTrue();
        GetPropertyValue<bool>(result, "IsFailure").Should().BeFalse();
    }

    [Fact]
    public void Result_failure_factory_should_capture_the_error()
    {
        var error = CreateError("result.failure", "Nope.", null);
        var result = InvokeFailureFactory(error);

        GetPropertyValue<bool>(result, "IsSuccess").Should().BeFalse();
        GetPropertyValue<bool>(result, "IsFailure").Should().BeTrue();
        GetPropertyValue<object>(result, "Error").Should().Be(error);
    }

    [Fact]
    public void Result_of_t_success_factory_should_capture_the_value()
    {
        var result = InvokeGenericSuccessFactory(typeof(string), "ok");

        GetPropertyValue<bool>(result, "IsSuccess").Should().BeTrue();
        GetPropertyValue<bool>(result, "IsFailure").Should().BeFalse();
        GetPropertyValue<string>(result, "Value").Should().Be("ok");
    }

    [Fact]
    public void Result_of_t_failure_factory_should_capture_the_error()
    {
        var error = CreateError("result.failure", "Still nope.", null);
        var result = InvokeGenericFailureFactory(typeof(string), error);

        GetPropertyValue<bool>(result, "IsSuccess").Should().BeFalse();
        GetPropertyValue<bool>(result, "IsFailure").Should().BeTrue();
        GetPropertyValue<object>(result, "Error").Should().Be(error);
    }

    [Fact]
    public void Result_should_support_implicit_failure_conversion_from_error()
    {
        var error = CreateError("implicit.failure", "Converted from error.", null);
        var converted = InvokeImplicitOperator(GetResultType(), GetErrorType(), error);

        GetPropertyValue<bool>(converted, "IsSuccess").Should().BeFalse();
        GetPropertyValue<bool>(converted, "IsFailure").Should().BeTrue();
        GetPropertyValue<object>(converted, "Error").Should().Be(error);
    }

    [Fact]
    public void Result_of_t_should_support_implicit_success_and_failure_conversions()
    {
        var closedGenericResult = GetGenericResultType().MakeGenericType(typeof(string));
        var success = InvokeImplicitOperator(closedGenericResult, typeof(string), "done");
        var error = CreateError("implicit.failure", "Generic conversion failed.", null);
        var failure = InvokeImplicitOperator(closedGenericResult, GetErrorType(), error);

        GetPropertyValue<bool>(success, "IsSuccess").Should().BeTrue();
        GetPropertyValue<string>(success, "Value").Should().Be("done");
        GetPropertyValue<bool>(failure, "IsFailure").Should().BeTrue();
        GetPropertyValue<object>(failure, "Error").Should().Be(error);
    }

    [Fact]
    public void Result_should_use_value_equality()
    {
        var error = CreateError("same.error", "Same failure.", null);
        var success = InvokeSuccessFactory();
        var otherSuccess = InvokeSuccessFactory();
        var failure = InvokeFailureFactory(error);
        var otherFailure = InvokeFailureFactory(error);

        success.Should().Be(otherSuccess);
        success.GetHashCode().Should().Be(otherSuccess.GetHashCode());
        failure.Should().Be(otherFailure);
        failure.GetHashCode().Should().Be(otherFailure.GetHashCode());
        success.Should().NotBe(failure);
    }

    [Fact]
    public void Result_of_t_should_use_value_equality()
    {
        var error = CreateError("same.error", "Same failure.", null);
        var success = InvokeGenericSuccessFactory(typeof(int), 42);
        var otherSuccess = InvokeGenericSuccessFactory(typeof(int), 42);
        var failure = InvokeGenericFailureFactory(typeof(int), error);
        var otherFailure = InvokeGenericFailureFactory(typeof(int), error);

        success.Should().Be(otherSuccess);
        success.GetHashCode().Should().Be(otherSuccess.GetHashCode());
        failure.Should().Be(otherFailure);
        failure.GetHashCode().Should().Be(otherFailure.GetHashCode());
        success.Should().NotBe(failure);
    }

    private static Type GetResultType()
    {
        var type = DomainAssembly.GetType(ResultTypeName);
        type.Should().NotBeNull($"the {ResultTypeName} foundation type should exist in MoreSpeakers.Domain");
        return type!;
    }

    private static Type GetGenericResultType()
    {
        var type = DomainAssembly.GetType(GenericResultTypeName);
        type.Should().NotBeNull($"the {GenericResultTypeName} foundation type should exist in MoreSpeakers.Domain");
        return type!;
    }

    private static Type GetErrorType()
    {
        var type = DomainAssembly.GetType(ErrorTypeName);
        type.Should().NotBeNull($"the {ErrorTypeName} foundation type should exist in MoreSpeakers.Domain");
        return type!;
    }

    private static object CreateError(string code, string message, Exception? exception)
    {
        var errorType = GetErrorType();
        var constructor = errorType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .SingleOrDefault(candidate =>
            {
                var parameters = candidate.GetParameters();
                return parameters.Length == 3
                       && parameters[0].ParameterType == typeof(string)
                       && parameters[1].ParameterType == typeof(string)
                       && typeof(Exception).IsAssignableFrom(parameters[2].ParameterType);
            });

        constructor.Should().NotBeNull("Error should expose a public constructor accepting code, message, and an optional exception");

        return constructor!.Invoke(new object?[] { code, message, exception });
    }

    private static object InvokeSuccessFactory()
        => GetStaticMethod(GetResultType(), "Success", false, Type.EmptyTypes).Invoke(null, null)!;

    private static object InvokeFailureFactory(object error)
        => GetStaticMethod(GetResultType(), "Failure", false, GetErrorType()).Invoke(null, new[] { error })!;

    private static object InvokeGenericSuccessFactory(Type valueType, object value)
        => GetGenericFactoryMethod(GetResultType(), "Success", parameter => parameter.ParameterType.IsGenericParameter)
            .MakeGenericMethod(valueType)
            .Invoke(null, new[] { value })!;

    private static object InvokeGenericFailureFactory(Type valueType, object error)
        => GetGenericFactoryMethod(GetResultType(), "Failure", parameter => parameter.ParameterType == GetErrorType())
            .MakeGenericMethod(valueType)
            .Invoke(null, new[] { error })!;

    private static object InvokeImplicitOperator(Type targetType, Type parameterType, object value)
    {
        var method = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
            {
                var parameters = candidate.GetParameters();
                return candidate.Name == "op_Implicit"
                       && candidate.ReturnType == targetType
                       && parameters.Length == 1
                       && parameters[0].ParameterType == parameterType;
            });

        method.Should().NotBeNull($"expected {targetType} to declare an implicit conversion from {parameterType}");

        return method!.Invoke(null, new[] { value })!;
    }

    private static MethodInfo GetStaticMethod(Type declaringType, string name, bool isGenericMethodDefinition, params Type[] parameterTypes)
    {
        var method = declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
            {
                var parameters = candidate.GetParameters();
                return candidate.Name == name
                       && candidate.IsGenericMethodDefinition == isGenericMethodDefinition
                       && parameters.Length == parameterTypes.Length
                       && parameters.Zip(parameterTypes).All(pair => pair.First.ParameterType == pair.Second);
            });

        method.Should().NotBeNull($"expected {declaringType} to declare {name}({string.Join(", ", parameterTypes.Select(type => type.Name))})");

        return method!;
    }

    private static MethodInfo GetGenericFactoryMethod(Type declaringType, string name, Func<ParameterInfo, bool> parameterMatcher)
    {
        var method = declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
            {
                var parameters = candidate.GetParameters();
                return candidate.Name == name
                       && candidate.IsGenericMethodDefinition
                       && candidate.GetGenericArguments().Length == 1
                       && parameters.Length == 1
                       && parameterMatcher(parameters[0]);
            });

        method.Should().NotBeNull($"expected {declaringType} to declare a generic {name} factory");

        return method!;
    }

    private static PropertyInfo GetRequiredProperty(Type declaringType, string propertyName)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        property.Should().NotBeNull($"expected {declaringType} to expose a public {propertyName} property");
        return property!;
    }

    private static PropertyInfo GetExceptionProperty(Type declaringType)
    {
        var property = declaringType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .SingleOrDefault(candidate => typeof(Exception).IsAssignableFrom(candidate.PropertyType));

        property.Should().NotBeNull("Error should expose a public exception property for optional diagnostic context");

        return property!;
    }

    private static T GetPropertyValue<T>(object instance, string propertyName)
        => (T)GetRequiredProperty(instance.GetType(), propertyName).GetValue(instance)!;
}
