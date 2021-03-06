using System;
using System.Threading;
using System.Threading.Tasks;
using Confix.Authoring.GraphQL.DataLoaders;
using Confix.Authoring.Store;
using HotChocolate.Types;
using HotChocolate.Types.Relay;

namespace Confix.Authoring.GraphQL.Applications
{
    [Node]
    [ExtendObjectType(typeof(Application))]
    public class ApplicationNode
    {
        [NodeResolver]
        public static Task<Application?> GetApplicationAsync(
            Guid id,
            ApplicationByIdDataLoader applicationById,
            CancellationToken cancellationToken) =>
            applicationById.LoadAsync(id, cancellationToken);
    }
}
