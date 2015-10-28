using System;

/// <summary>
/// Use this utility class to compute scores for different paths in a VRN environment
/// </summary>
public class VRNScoring
{
	/// <summary>
	/// Represent the different ways of scoring a path
	/// </summary>
	public enum VRNScoreType
	{
		/// <summary>
		/// Normalize with respect to short distance
		/// </summary>
		SIMPLE_DIST,

		/// <summary>
		/// Algorithm used by Byagowi in the paper
		/// 'Design of a Virtual Reality Navigational (VRN) Experiment for Assessment of Egocentric Spatial Cognition'
		/// as published in the 2012 IEEE EMBC
		/// </summary>
		BYAGOWI_1
	}

	/// <summary>
	/// Compute the score of a path with respect to a minimal distance path
	/// </summary>
	/// <param name="path">the path to score</param>
	/// <param name="minPath">the shortest possible route to get between the start and end point</param>
	/// <param name="scoreType">which algorithm should be used to compute the score</param>
	/// <returns>a double score that compares this path to minPath.</returns>
	/// <throws>InvalidOperationException if scoreType is unrecognized</throws>
	public static double Score(VRNPath path, VRNPath minPath, VRNScoreType scoreType)
	{
		double mScore = 0.0;

		switch (scoreType)
		{
			case VRNScoreType.SIMPLE_DIST:
				mScore = path.distance / minPath.distance;
				break;

			case VRNScoreType.BYAGOWI_1:
				mScore = 0;
				break;

			default:
				throw new InvalidOperationException("Unrecognized algorithm");
		}

		return mScore;
	}
}