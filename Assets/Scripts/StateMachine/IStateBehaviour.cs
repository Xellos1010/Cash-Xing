using System;
using UnityEngine;

	public interface IStateBehaviour<TStateMachine> {
		void InitializeWithContext(Animator animator, TStateMachine stateMachine);
		void Enable();
		void Disable();
	}
