using UnityEngine;

/// <summary>
/// Receives the Footstep Animation Event placed on the frames of the Walk clip where a foot lands,
/// and plays a footstep sound with a small puff of dust. Must sit on the same GameObject as the Animator.
/// </summary>
public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private int dustPerStep = 3;
    [SerializeField] private AudioClip footstepSound;

    /// <summary>
    /// Called by the Animation Event. Inside a blend tree, events fire from every clip being mixed,
    /// so steps are ignored unless the Walk clip is the main one playing.
    /// </summary>
    public void Footstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight < 0.5f)
        {
            return;
        }

        dust.Emit(dustPerStep);
        AudioManager.PlaySound(footstepSound);
    }
}