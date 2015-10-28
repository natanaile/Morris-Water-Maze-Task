using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Oasis : GotoTask
{

	private AudioSource mAudioSource;
	private ParticleSystem mParticleSystem;

	// Use this for initialization
 	void Start()
	{
		base.Start();

		mAudioSource = GetComponent<AudioSource>();
		mAudioSource.Stop();

		mParticleSystem = GetComponentInChildren<ParticleSystem>();
		if (null != mParticleSystem)
		{
			mParticleSystem.Stop();
		}

		VRNWaterTaskSettings settings = VRNWaterTaskSettings.Load();
		this.latency = settings.timeToComplete;

		this.Initialize(this.name, true);
	}

	// OnTriggerEnter is called when the Collider other enters the trigger
	public override void OnTriggerEnter(Collider other)
	{
		base.OnTriggerEnter(other);

		// start playing music and add particle effect
		mAudioSource.Play();

		if (null != mParticleSystem)
		{
			mParticleSystem.Play();
		}
	}

	// OnTriggerExit is called when the Collider other has stopped touching the trigger
	public void OnTriggerExit(Collider other)
	{
		base.OnTriggerExit(other);

		mAudioSource.Stop();
		if (null != mParticleSystem)
		{
			mParticleSystem.Stop();
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
