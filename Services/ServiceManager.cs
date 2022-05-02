using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase.Services
{
	public class ServiceManager 
	{
		public static ServiceManager Instance;

		private Dictionary<string, Service> services = new Dictionary<string, Service>();
		private Dictionary<Type, Service> servicesByType = new Dictionary<Type, Service>();
		private List<Service> iterableServies = new List<Service>();

		public ServiceManager()
		{
			if (Instance == null)
				Instance = this;
			else throw new Exception("Cannot construct more than one ServiceManager.");
		}

		public void Update(DeltaTime delta)
		{
			foreach (Service service in iterableServies)
			{
				service.Update(delta);
			}
		}

		public void AddService(string serviceName, Service service)
		{
			services.Add(serviceName, service);
			iterableServies.Add(service);

			servicesByType.Add(service.GetType(), service);
		}

		public Service GetService(string serviceName)
		{
			services.TryGetValue(serviceName, out Service service);

			return service;
		}

		public T GetService<T>() where T : Service
		{
			if (servicesByType.TryGetValue(typeof(T), out Service service))
			{
				return service as T;
			}

			return default;
		}
	}
}
