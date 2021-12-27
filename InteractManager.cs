using BRVBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class InteractManager
	{
		public delegate void InteractHover();
		public delegate void InteractSuccess();
		private int previousPriority;

		private InteractHover interactHover;
		private InteractSuccess interactSuccess;

		//Specifies the time after which interaction is next valid.
		private float interactionTime;

		private readonly Input input;

		public InteractManager(Input input)
		{
			this.input = input;
		}

		public void Add(int priority, InteractHover interactHover, InteractSuccess interactSuccess)
		{
			if (priority < previousPriority)
			{
				previousPriority = priority;
				this.interactHover = interactHover;
				this.interactSuccess = interactSuccess;
			}
		}

		public void Invoke(DeltaTime delta)
		{
			interactHover?.Invoke();

			if (delta.Now > interactionTime && input.IsKeyJustPressed(Veldrid.Key.F))
			{
				interactSuccess?.Invoke();
				interactionTime = (float)delta.Now + 0.5f;
			}

			previousPriority = int.MaxValue;
			interactSuccess = null;
			interactHover = null;
		}
	}
}
