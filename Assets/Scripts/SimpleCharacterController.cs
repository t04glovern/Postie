using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
    [Header("Controls")]
    public string XAxis = "Horizontal";
    public string YAxis = "Vertical";
    public string JumpButton = "Jump";
    public string SlapButton = "Fire1";
    public string WobbleHatButton = "Fire2";

    [Header("Moving")]
    public float walkSpeed = 4;
    public float runSpeed = 10;
    public float gravity = 65;

    [Header("Jumping")]
    public float jumpSpeed = 25;
    public float jumpDuration = 0.5f;
    public float jumpInterruptFactor = 100;

    [Header("Graphics")]
    public Transform graphicsRoot;
    public SkeletonAnimation skeletonAnimation;

    [Header("Animation")]
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string crouchName = "crouch";
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string hatWobbleName = "hat-wobble";
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string idleName = "idle";
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string jumpName = "jump";
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string slapName = "slap";
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string walkName = "walk";
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string runName = "run";

    [Header("Audio")]
    public AudioClip walkAudioClip;
    public AudioClip jumpAudioClip;
    public AudioClip[] slapAudioClips;
    public AudioClip[] hatWobbleAudioClips;
    public AudioSource playerAudioSource;

    [Header("Events")]
    [SpineEvent]
    public string hatWobbleEventName = "hat-wobble";
    [SpineEvent]
    public string jumpLandEventName = "jump-land";
    [SpineEvent]
    public string jumpStartEventName = "jump-start";
    [SpineEvent]
    public string slapStartEventName = "slap-start";
    [SpineEvent]
    public string spinStartEventName = "spin-start";
    [SpineEvent]
    public string walkTapEventName = "walk-tap";
    [SpineEvent]
    public string runningTapEventName = "run-tap";

    [Header("Key Bones")]
    [SpineBone]
    public Spine.Bone armBone;
    [SpineBone]
    public Spine.Bone headBone;

    CharacterController controller;
    ShootProjectile shootProjectile;

    Vector2 velocity = Vector2.zero;
    Vector2 lastVelocity = Vector2.zero;
    bool lastGrounded = false;
    float jumpEndTime = 0;
    bool jumpInterrupt = false;
    bool facingRight = true;

    private float _timeElapsed = 0f;
    float idleTime = 0.5f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        shootProjectile = GetComponent<ShootProjectile>();
    }

    void Start()
    {
        // Register a callback for Spine Events (in this case, Footstep)
        skeletonAnimation.state.Event += HandleEvent;
    }

    void HandleEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        // Play some sound if walking-step event fired
        if (e.Data.Name == walkTapEventName)
        {
            playerAudioSource.Stop();
            playerAudioSource.clip = walkAudioClip;
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();
        }

        // Play some sound if running-step event fired
        if (e.Data.Name == runningTapEventName)
        {
            playerAudioSource.Stop();
            playerAudioSource.clip = walkAudioClip;
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();
        }
    }

    void Update()
    {
        armBone = skeletonAnimation.skeleton.FindBone("Arm-Left");
        headBone = skeletonAnimation.skeleton.FindBone("Mouth");

        Vector3 armBoneTransform = transform.TransformPoint(
            new Vector3(armBone.WorldX, armBone.WorldY, 0));
        Vector3 headBoneTransform = transform.TransformPoint(
            new Vector3(headBone.WorldX, headBone.WorldY, 0));

        bool slapping = false;
        bool wobbling = false;

        //control inputs
        float x = Input.GetAxis(XAxis);
        float y = Input.GetAxis(YAxis);

        if (Input.GetButton(SlapButton))
        {
            slapping = true;
        }
        if (Input.GetButton(WobbleHatButton))
        {
            wobbling = true;
        }

        velocity.x = 0;

        if (Input.GetButtonDown(JumpButton) && controller.isGrounded)
        {
            //jump
            playerAudioSource.Stop();
            playerAudioSource.clip = jumpAudioClip;
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();
            velocity.y = jumpSpeed;
            jumpEndTime = Time.time + jumpDuration;
        }
        else if (Time.time < jumpEndTime && Input.GetButtonUp(JumpButton))
        {
            jumpInterrupt = true;
        }


        if (x != 0)
        {
            //walk or run
            velocity.x = Mathf.Abs(x) > 0.6f ? runSpeed : walkSpeed;
            velocity.x *= Mathf.Sign(x);
        }

        if (jumpInterrupt)
        {
            //interrupt jump and smoothly cut Y velocity
            if (velocity.y > 0)
            {
                velocity.y = Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 100);
            }
            else
            {
                jumpInterrupt = false;
            }
        }

        //apply gravity F = mA (Learn it, love it, live it)
        velocity.y -= gravity * Time.deltaTime;

        //move
        controller.Move(new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime);

        if (controller.isGrounded)
        {
            //cancel out Y velocity if on ground
            velocity.y = -gravity * Time.deltaTime;
            jumpInterrupt = false;
        }

        //graphics updates
        // slapping
        if (slapping && _timeElapsed > idleTime)
        {
            // slap
            playerAudioSource.Stop();
            playerAudioSource.clip = GetRandomSlap();
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();

            skeletonAnimation.loop = false;
            skeletonAnimation.AnimationName = slapName;

            shootProjectile.Fire(facingRight, armBoneTransform);

            _timeElapsed = 0;
        }
        // wobbling hat
        else if (wobbling && _timeElapsed > idleTime)
        {
            // wobble
            playerAudioSource.Stop();
            playerAudioSource.clip = GetRandomWobble();
            playerAudioSource.pitch = GetRandomPitch(0.2f);
            playerAudioSource.Play();

            skeletonAnimation.loop = false;
            skeletonAnimation.AnimationName = hatWobbleName;

            _timeElapsed = 0;
        }
        // idle
        else
        {
            _timeElapsed += Time.deltaTime;

            if (controller.isGrounded && _timeElapsed > idleTime)
            {
                skeletonAnimation.loop = true;
                if (x == 0) //idle
                {
                    skeletonAnimation.AnimationName = idleName;
                }
                else //move
                {
                    skeletonAnimation.AnimationName = Mathf.Abs(x) > 0.6f ? runName : walkName;
                }
            }
        }

        if (!controller.isGrounded)
        {
            skeletonAnimation.loop = false;
            if (velocity.y > 0) //jump
            {
                skeletonAnimation.AnimationName = jumpName;
            }
            else //fall
            {
                skeletonAnimation.AnimationName = jumpName;
            }
        }

        //flip left or right
        if (x > 0)
        {
            facingRight = true;
            skeletonAnimation.skeleton.FlipX = false;
        }
        else if (x < 0)
        {
            facingRight = false;
            skeletonAnimation.skeleton.FlipX = true;
        }

        //store previous state
        lastVelocity = velocity;
        lastGrounded = controller.isGrounded;
    }

    static float GetRandomPitch(float maxOffset)
    {
        return 1f + Random.Range(-maxOffset, maxOffset);
    }

    AudioClip GetRandomSlap()
    {
        return slapAudioClips[Random.Range(0,(slapAudioClips.Length - 1))];
    }

    AudioClip GetRandomWobble()
    {
        return hatWobbleAudioClips[Random.Range(0, (hatWobbleAudioClips.Length - 1))];
    }
}
