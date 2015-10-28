using System.Collections;
using UnityEngine;

/// <summary>
/// Allow delayed execution of functions by means of Unity Co-routines
/// </summary>
public class DelayedExecution
	{
		public delegate void DelayedFunctionArgs(object[] args);		
		private static IEnumerator DelayExecutionCoroutine(float seconds, DelayedFunctionArgs functionToDelay, params object[] functionArgs)
		{
			yield return new WaitForSeconds(seconds);
			functionToDelay(functionArgs);
		}

		public delegate void DelayedFunction();
		private static IEnumerator DelayExecutionCoroutine(float seconds, DelayedFunction functionToDelay)
		{
			yield return new WaitForSeconds(seconds);
			functionToDelay();
		}

		/// <summary>
		/// execute a function after a specific amount of time using a co-routine
		/// </summary>
		/// <param name="seconds"></param>
		/// <param name="functionToDelay"></param>
		/// <returns></returns>
		public static void DelayExecution(MonoBehaviour behaviour, float seconds, DelayedFunctionArgs functionToDelay, params object[] functionArgs)
		{
			behaviour.StartCoroutine(DelayExecutionCoroutine(seconds, functionToDelay, functionArgs)); 
		}

		public static void DelayExecution(MonoBehaviour behaviour, float seconds, DelayedFunction functionToDelay)
		{
			behaviour.StartCoroutine(DelayExecutionCoroutine(seconds, functionToDelay)); 
		}
	}