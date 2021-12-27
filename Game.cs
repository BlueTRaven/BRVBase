using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace BRVBase
{
	public abstract class Game
	{
		public static bool Exit;

		protected GraphicsDevice device;
		protected ResourceFactory factory;

		public Game()
		{
		}

		public virtual void LoadContent(GraphicsDevice device, ResourceFactory factory) 
		{
			this.device = device;
			this.factory = factory;
		}

		public virtual void Initialize(Sdl2Window window) 
		{
		}

		public virtual void Update(DeltaTime delta) { }

		public virtual void Draw(DeltaTime delta) { }
	}
}
