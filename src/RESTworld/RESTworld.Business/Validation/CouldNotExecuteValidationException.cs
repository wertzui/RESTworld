using System;
using System.Runtime.Serialization;

namespace RESTworld.Business.Validation
{
    /// <summary>
    /// An exception that occurred during validation.
    /// </summary>
    public class CouldNotExecuteValidationException : Exception
    {
        private CouldNotExecuteValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        protected CouldNotExecuteValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Creates a new <see cref="CouldNotExecuteValidationException"/>.
        /// </summary>
        /// <typeparam name="TCreateDto">The type of the create DTO being validated.</typeparam>
        /// <typeparam name="TUpdateDto">The type of the update DTO being validated.</typeparam>
        /// <typeparam name="TEntity">The type of the entity being validated.</typeparam>
        /// <param name="innerException">The exception that occurred during the validation.</param>
        public static CouldNotExecuteValidationException Create<TCreateDto, TUpdateDto, TEntity>(Exception innerException)
        {
            var message = $"An error occurred during the execution of the validation for the DTO {typeof(TCreateDto).Name} or {typeof(TUpdateDto).Name} and the entity {typeof(TEntity).Name}. See the InnerException for details.";

            return new CouldNotExecuteValidationException(message, innerException);
        }
    }
}
