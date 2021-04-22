using System;
using System.Linq;
using System.Net;

namespace RESTworld.Business
{
    public static class AuthorizationResult
    {
        public static AuthorizationResult<TEntity> FromStatus<TEntity>(HttpStatusCode status) => new AuthorizationResult<TEntity>(status);

        public static AuthorizationResult<TEntity, T1> FromStatus<TEntity, T1>(HttpStatusCode status, T1 value1) => new AuthorizationResult<TEntity, T1>(status, value1);

        public static AuthorizationResult<TEntity, T1, T2> FromStatus<TEntity, T1, T2>(HttpStatusCode status, T1 value1, T2 value2) => new AuthorizationResult<TEntity, T1, T2>(status, value1, value2);

        public static AuthorizationResult<TEntity> Ok<TEntity>() => FromStatus<TEntity>(HttpStatusCode.OK);

        public static AuthorizationResult<TEntity, T1> Ok<TEntity, T1>(T1 value1) => FromStatus<TEntity, T1>(HttpStatusCode.OK, value1);

        public static AuthorizationResult<TEntity, T1, T2> Ok<TEntity, T1, T2>(T1 value1, T2 value2) => FromStatus<TEntity, T1, T2>(HttpStatusCode.OK, value1, value2);

        public static AuthorizationResult<TEntity> Forbidden<TEntity>() => FromStatus<TEntity>(HttpStatusCode.Forbidden);

        public static AuthorizationResult<TEntity, T1> Forbidden<TEntity, T1>(T1 value1) => FromStatus<TEntity, T1>(HttpStatusCode.Forbidden, value1);

        public static AuthorizationResult<TEntity, T1, T2> Forbidden<TEntity, T1, T2>(T1 value1, T2 value2) => FromStatus<TEntity, T1, T2>(HttpStatusCode.Forbidden, value1, value2);

        public static AuthorizationResult<TEntity> Unauthorized<TEntity>() => FromStatus<TEntity>(HttpStatusCode.Unauthorized);

        public static AuthorizationResult<TEntity, T1> Unauthorized<TEntity, T1>(T1 value1) => FromStatus<TEntity, T1>(HttpStatusCode.Unauthorized, value1);

        public static AuthorizationResult<TEntity, T1, T2> Unauthorized<TEntity, T1, T2>(T1 value1, T2 value2) => FromStatus<TEntity, T1, T2>(HttpStatusCode.Unauthorized, value1, value2);
    }

    public class AuthorizationResult<TEntity>
    {
        private static Func<IQueryable<TEntity>, IQueryable<TEntity>> _defaultFilter = source => source;

        public AuthorizationResult(HttpStatusCode status, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null)
        {
            Status = status;

            if (filter is null)
                Filter = _defaultFilter;
            else
                Filter = filter;
        }

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }
        public HttpStatusCode Status { get; }
    }

    public class AuthorizationResult<TEntity, T1> : AuthorizationResult<TEntity>
    {
        public AuthorizationResult(HttpStatusCode status, T1 value1, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null) : base(status, filter)
        {
            Value1 = value1;
        }

        public T1 Value1 { get; }
    }

    public class AuthorizationResult<TEntity, T1, T2> : AuthorizationResult<TEntity, T1>
    {
        public AuthorizationResult(HttpStatusCode status, T1 value1, T2 value2, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null) : base(status, value1, filter)
        {
            Value2 = value2;
        }

        public T2 Value2 { get; }
    }
}