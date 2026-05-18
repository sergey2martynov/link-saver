using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Gateway.Web.Transformers;

public class UserIdHeaderTransform : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context) { }
    public void ValidateCluster(TransformClusterValidationContext context) { }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(ctx =>
        {
            var userId = ctx.HttpContext.User.FindFirst("sub")?.Value;
            if (userId is not null)
                ctx.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
            return ValueTask.CompletedTask;
        });
    }
}
