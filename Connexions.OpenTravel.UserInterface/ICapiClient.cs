using System.Threading;
using System.Threading.Tasks;

namespace Connexions.OpenTravel.UserInterface
{
	interface ICapiClient
	{
		Task<T> PostAsync<T>(string path, object body, CancellationToken cancellationToken);
	}
}