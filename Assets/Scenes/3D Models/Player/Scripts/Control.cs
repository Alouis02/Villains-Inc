using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour
{
    // Player animation
    private Animator animate;

    // Player Animation Hashes
    private int WalkingHash;
    private int RunningHash;
    private int RunStopHash;
    private int WalkingBackHash;
    private int WalkingLeftHash;
    private int WalkingRightHash;
    private int JumpingHash;
    private int RunJumpingHash;

    private int FlyingHash;
    private int DescendingHash;
    private int AscendingHash;
    private int Slashing1Hash;
    private int Slashing2Hash;
    private int Slashing3Hash;
    private int JumpAttackingHash;

    private bool isFlying;

    void Start()
    {
        animate = GetComponent<Animator>();
        if (animate == null) { Debug.LogError("Animator component not found!"); return; }

        // Initialize hashes
        WalkingHash = Animator.StringToHash("IsWalking");
        RunningHash = Animator.StringToHash("IsRunning");
        RunStopHash = Animator.StringToHash("IsRunStopping");
        WalkingBackHash = Animator.StringToHash("IsWalkingBack");
        WalkingLeftHash = Animator.StringToHash("IsWalkingLeft");
        WalkingRightHash = Animator.StringToHash("IsWalkingRight");
        JumpingHash = Animator.StringToHash("IsJumping");
        RunJumpingHash = Animator.StringToHash("IsRunJumping");
        FlyingHash = Animator.StringToHash("IsFlying");
        DescendingHash = Animator.StringToHash("IsDescending");
        AscendingHash = Animator.StringToHash("IsAscending");
        Slashing1Hash = Animator.StringToHash("IsSlash1");
        Slashing2Hash = Animator.StringToHash("IsSlash2");
        Slashing3Hash = Animator.StringToHash("IsSlash3");
        JumpAttackingHash = Animator.StringToHash("IsJumpAttack");
    }

    void Update()
    {
        HandleMovement();
        HandleActions();
    }

    void HandleMovement()
    {
        bool walk = Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow);
        bool run = Input.GetKey(KeyCode.LeftShift) && Input.GetKey("w");
        bool runstop = Input.GetKeyUp(KeyCode.LeftShift);
        bool walkback = Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow);
        bool walkL = Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow);
        bool walkR = Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool runjump = Input.GetKey("w") && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space);
        bool fmode = Input.GetKeyDown(KeyCode.Alpha1);
        if (fmode)
        {
            isFlying = !isFlying;
            animate.SetBool("IsFlying", isFlying);
        }
        bool fly = Input.GetKeyDown("w") || Input.GetKey("s") || Input.GetKey("a") || Input.GetKey("d");
        bool ascend = Input.GetKey(KeyCode.Space);
        bool descend = Input.GetKey(KeyCode.LeftShift);
        

        animate.SetBool(WalkingHash, walk);
        animate.SetBool(RunningHash, run);
        animate.SetBool(RunStopHash, runstop);
        animate.SetBool(WalkingBackHash, walkback);
        animate.SetBool(WalkingLeftHash, walkL);
        animate.SetBool(WalkingRightHash, walkR);
        animate.SetBool(JumpingHash, jump);    
        animate.SetBool(RunJumpingHash, runjump);   
        animate.SetBool(DescendingHash, descend); 
        animate.SetBool(AscendingHash, ascend); 
    }

    void HandleActions()
    {
        bool slash1 = Input.GetMouseButtonDown(0);
        bool slash2 = Input.GetMouseButtonDown(0);
        bool slash3 = Input.GetMouseButtonDown(0);
        bool jumpAttack = Input.GetMouseButtonDown(1);

        //bool aim = Input.GetMouseButton(1);
        //bool fire = Input.GetMouseButtonDown(0);

        // Combat animations
        animate.SetBool(Slashing1Hash, slash1);
        animate.SetBool(Slashing2Hash, slash2);
        animate.SetBool(Slashing3Hash, slash3);
        animate.SetBool(JumpAttackingHash, jumpAttack);

        //Aiming
        // animate.SetBool(AimingHash, aim);

        //Firing
        // animate.SetBool(FiringHash, fire);
    }
}