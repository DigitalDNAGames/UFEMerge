using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using FPLibrary;
using UFE3D;
using System.Linq;
using MovementEffects;

public class ControlsScript : MonoBehaviour
{

	#region trackable definitions
	public Fix64 afkTimer;
	public int airJuggleHits;
	public AirRecoveryType airRecoveryType;
	public bool applyRootMotion;
	public bool blockStunned;
	public Fix64 currentLifePoints;
	public Fix64[] currentGaugesPoints;
	public Fix64 comboDamage;
	public Fix64 comboHitDamage;
	public int comboHits;
	public int consecutiveCrumple;
	public BasicMoveReference currentBasicMove;
	public CombatStances currentCombatStance;
	public Fix64 currentDrained;
	public string currentHitAnimation;
	public PossibleStates currentState;
	public SubStates currentSubState;

	public MoveInfo DCMove;
	//soonk - addition
	public MoveInfo moveOnConnect;
	public CombatStances DCStance;
	public MoveInfo enterMove;
	public MoveInfo exitMove;
	public bool firstHit;
	public Fix64 gaugeDPS;
	public GaugeId gaugeDrainId;
	public bool hitDetected;
	public Fix64 hitAnimationSpeed;
	public bool inhibitGainWhileDraining;
	public bool isAirRecovering;
	public bool isAssist;
	public bool isBlocking;
	public bool isDead;
	public bool ignoreCollisionMass;
	public bool introPlayed;
	public bool lit;
	public bool lockXMotion = false;
	public bool lockYMotion = false;
	public bool lockZMotion = false;
	public UFE3D.CharacterInfo myInfo;
	public int mirror;
	public Fix64 normalizedDistance;
	public Fix64 normalizedJumpArc;
	public bool outroPlayed;
	public bool potentialBlock;
	public Fix64 potentialParry;
	public bool roundMsgCasted;
	public int roundsWon;
	public bool shakeCamera;
	public bool shakeCharacter;
	public Fix64 shakeDensity;
	public Fix64 shakeCameraDensity;
	public StandUpOptions standUpOverride;
	public Fix64 standardYRotation;
	public Fix64 storedMoveTime;
	public Fix64 stunTime;
	public Fix64 totalDrain;

	//soonk addition
	public bool tripGuard;
	private DefaultBattleGUI defaultBattleGUI;
	private bool timeOutDead = false;
	private bool hitWhileDead = false;
	public List<string> uniqueMoves = new List<string>();
	public List<string> moveTypes = new List<string>();
	int specialsDone = 0;
	bool standToDuck = false;
	bool duckToStand = false;
	bool wasDucking = false;
	bool dtsStarted = false;
	string standUpAnimTemp;
	float standUpTimeTemp;
	public bool downHeld = false;
	bool ncfa_active = false;
	public bool inRage = false;
	public int inRageTimes = 0;
	public PossibleSides currentSide;
	int armorHitFramesRemain = 0;
	float armorHitFlashFade = 0f;
	float armorFlash = 0;
	Color armorColor;
	float armorColorStart = 0f;
	public bool blockPressed = false;
	bool sakataReady = false;
	public bool smokeBombHit = false;
	bool projCasted = false;
	bool tempDead = false;

	public bool draw = false;

	public PullIn activePullIn;
	public Hit currentHit;
	public MoveInfo currentMove;
	public MoveInfo storedMove;
	public MoveInfo overrideFinsiher;
	public PhysicsScript Physics { get { return this.myPhysicsScript; } set { myPhysicsScript = value; } }
	public MoveSetScript MoveSet { get { return this.myMoveSetScript; } set { myMoveSetScript = value; } }
	public HitBoxesScript HitBoxes { get { return this.myHitBoxesScript; } set { myHitBoxesScript = value; } }

	public Dictionary<ButtonPress, Fix64> inputHeldDown = new Dictionary<ButtonPress, Fix64>();
	public List<ProjectileMoveScript> projectiles = new List<ProjectileMoveScript>();
	public List<ControlsScript> assists = new List<ControlsScript>();

	public FPTransform worldTransform;
	public FPTransform localTransform;
	#endregion


	public Shader[] normalShaders;
	public Color[] normalColors;

	public HeadLookScript headLookScript;
	public GameObject emulatedCam;
	public CameraScript cameraScript;

	//soonk addition
	public VideoPlayer finisherPlayer;
	public AudioSource finisherAudio;
	public Texture2D gameEndTexture;
	public AudioSource audioSource;
	private bool firstFinisherPlayed = false;
	private bool firstFinisherStarted = false;
	public bool finisherReady = false;
	public bool finisherEnded = false;

	// achievement trackers
	private bool hasBlocked = false;
	private bool hasJumped = false;
	public bool finisher1 = false;
	public bool finisher2 = false;
	public bool finisher3 = false;

	public Text debugger;
	public string aiDebugger { get; set; }
	public CharacterDebugInfo debugInfo;
	public int playerNum;
	public bool isAlt;
	public int selectedCostume = 0;

	[HideInInspector]
	public GameObject character;
	[HideInInspector]
	public GameObject opponent;
	[HideInInspector]
	public UFE3D.CharacterInfo opInfo;
	[HideInInspector]
	public ChallengeMode challengeMode;
	[HideInInspector]
	public ControlsScript opControlsScript;
	[HideInInspector]
	public MoveSetData[] loadedMoves;
	[HideInInspector]
	public ControlsScript owner { get { return isAssist ? _owner : this; } set { _owner = value; } }


	private PhysicsScript myPhysicsScript;
	private MoveSetScript myMoveSetScript;
	private HitBoxesScript myHitBoxesScript;
	private ControlsScript _owner;


	void Start()
	{

		foreach (ButtonPress bp in System.Enum.GetValues(typeof(ButtonPress)))
		{
			inputHeldDown.Add(bp, 0);
		}

		cameraScript = transform.parent.GetComponent<CameraScript>();

		// Assign Opponent
		if (playerNum == 1)
		{
			UFE.usbHandler.tempUSBinfo();
			opponent = GameObject.Find("Player2");
			opInfo = UFE.config.player2Character;
			mirror = -1;
			playerNum = 1;
			myInfo.playerSide = 1;
			currentSide = PossibleSides.P1;

			/*
            Camera overlayCam = UFE.overlayCam.GetComponent<Camera>();
            overlayCam.enabled = true;
            
            foreach (var component in UFE.overlayCam.GetComponents<VideoPlayer>())
            {
                Destroy(component);
            }
            */

		}
		else if (playerNum == 2)
		{
			opponent = GameObject.Find("Player1");
			opInfo = UFE.config.player1Character;
			mirror = 1;
			playerNum = 2;
			myInfo.playerSide = 2;
			currentSide = PossibleSides.P2;
		}
		opControlsScript = opponent.GetComponent<ControlsScript>();

		//soonk addition
		myInfo.playerNum = playerNum;
		defaultBattleGUI = FindObjectOfType<DefaultBattleGUI>();


		// Set Alternative Costume
		if (!isAssist)
		{
			if (isAlt)
			{
				if (myInfo.alternativeCostumes[selectedCostume].enableColorMask)
				{
					Renderer[] charRenders = character.GetComponentsInChildren<Renderer>();
					foreach (Renderer charRender in charRenders)
					{
						charRender.material.color = myInfo.alternativeCostumes[selectedCostume].colorMask;
					}
				}
			}

			Renderer[] charRenderers = character.GetComponentsInChildren<Renderer>();
			List<Shader> shaderList = new List<Shader>();
			List<Color> colorList = new List<Color>();
			foreach (Renderer char_rend in charRenderers)
			{
				shaderList.Add(char_rend.material.shader);
				colorList.Add(char_rend.material.color);
			}
			normalShaders = shaderList.ToArray();
			normalColors = colorList.ToArray();
		}


		// Head Movement
		if (myInfo.headLook.enabled)
		{
			character.AddComponent<HeadLookScript>();
			headLookScript = character.GetComponent<HeadLookScript>();
			headLookScript.segments = myInfo.headLook.segments;
			headLookScript.nonAffectedJoints = myInfo.headLook.nonAffectedJoints;
			headLookScript.effect = myInfo.headLook.effect;
			headLookScript.overrideAnimation = !myInfo.headLook.overrideAnimation;

			foreach (BendingSegment segment in headLookScript.segments)
			{
				segment.firstTransform = myHitBoxesScript.GetTransform(segment.bodyPart).parent.transform;
				segment.lastTransform = myHitBoxesScript.GetTransform(segment.bodyPart);
			}

			foreach (NonAffectedJoints nonAffectedJoint in headLookScript.nonAffectedJoints)
				nonAffectedJoint.joint = myHitBoxesScript.GetTransform(nonAffectedJoint.bodyPart);
		}


		// Challenge Mode
		if (!isAssist && playerNum == 1 && UFE.gameMode == GameMode.ChallengeMode)
		{
			challengeMode = gameObject.AddComponent<ChallengeMode>();
			challengeMode.cScript = this;
		}


		// Rotate and start idle animation
		//soonk addition
		/*
        if (playerNum == 2) testCharacterRotation(0, true);
        myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
        */
		if (currentSide == PossibleSides.P1)
		{
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
		}
		else
		{
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idleP2);
		}
	}

	private bool isAxisRested(IDictionary<InputReferences, InputEvents> currentInputs)
	{
		if (currentState == PossibleStates.Down) return true;
		if (UFE.config.lockMovements) return true;
		foreach (InputReferences inputRef in currentInputs.Keys)
		{
			if (inputRef.inputType == InputType.Button) continue;
			if (currentInputs[inputRef].axisRaw != 0)
			{
				if (inputRef.inputType == InputType.HorizontalAxis && !myMoveSetScript.basicMoves.moveEnabled) return true;
				if (inputRef.inputType == InputType.VerticalAxis)
				{
					if (currentInputs[inputRef].axisRaw > 0 && !myMoveSetScript.basicMoves.jumpEnabled) return true;
					if (currentInputs[inputRef].axisRaw < 0 && !myMoveSetScript.basicMoves.crouchEnabled) return true;
				}
			}
		}
		return true;
	}

	public void ForceMirror(bool toggle)
	{
		if (myInfo.animationType == AnimationType.Legacy)
		{
			float xScale = Mathf.Abs(character.transform.localScale.x) * (toggle ? -1 : 1);
			character.transform.localScale = new Vector3(xScale, character.transform.localScale.y, character.transform.localScale.z);
		}
		else
		{
			myMoveSetScript.SetMecanimMirror(toggle);
			if (!myInfo.useAnimationMaps) myHitBoxesScript.InvertHitBoxes(toggle);
		}
	}

	public void InvertRotation(int newMirror = 0)
	{
		mirror = newMirror != 0 ? newMirror : mirror *= -1;
		standardYRotation = FPMath.Abs(standardYRotation) * -mirror;
	}

	private void testCharacterRotation()
	{
		testCharacterRotation(0, false);
	}

	private void testCharacterRotation(Fix64 rotationSpeed, bool forceMirror = false)
	{
		if ((mirror == -1 || forceMirror) && worldTransform.position.x > opControlsScript.worldTransform.position.x)
		{
			mirror = 1;
			potentialBlock = false;
			InvertRotation(1);
			if (UFE.config.characterRotationOptions.autoMirror) ForceMirror(true);
			myHitBoxesScript.inverted = true;
			//soonk addition
			myInfo.playerSide = 2;
			currentSide = PossibleSides.P2;
			UFE.FireSideSwitch(mirror, this);

		}
		else if ((mirror == 1 || forceMirror) && worldTransform.position.x < opControlsScript.worldTransform.position.x)
		{
			mirror = -1;
			potentialBlock = false;
			InvertRotation(-1);
			if (UFE.config.characterRotationOptions.autoMirror) ForceMirror(false);
			myHitBoxesScript.inverted = false;
			//soonk addition
			myInfo.playerSide = 1;
			currentSide = PossibleSides.P1;
			UFE.FireSideSwitch(mirror, this);
		}

		if (rotationSpeed == 0 ||
			(UFE.config.networkOptions.disableRotationBlend && (UFE.isConnected || UFE.config.debugOptions.emulateNetwork)))
		{
			fixCharacterRotation();

		}
		else
		{
			FPQuaternion newRotation = FPQuaternion.Slerp(
				localTransform.rotation,
				FPQuaternion.AngleAxis(standardYRotation, FPVector.up),
				(UFE.fixedDeltaTime * rotationSpeed)
			);

			if (newRotation.ToString() != new FPQuaternion(0, 0, 0, 0).ToString()) localTransform.rotation = newRotation;
		}
	}

	private void fixCharacterRotation()
	{
		if (currentState == PossibleStates.Down) return;

		FPQuaternion fixedRotation = FPQuaternion.AngleAxis(standardYRotation, FPVector.up);
		localTransform.rotation = fixedRotation;
	}

	private void validateRotation()
	{
		if (!myPhysicsScript.IsGrounded() || myPhysicsScript.freeze || currentMove != null) fixCharacterRotation();

		if (myPhysicsScript.freeze) return;
		if (currentState == PossibleStates.Down) return;
		if (currentMove != null) return;
		if (myPhysicsScript.IsJumping() && !UFE.config.characterRotationOptions.rotateWhileJumping) return;
		if (currentSubState == SubStates.Stunned && !UFE.config.characterRotationOptions.fixRotationWhenStunned) return;
		if (isBlocking && !UFE.config.characterRotationOptions.fixRotationWhenBlocking) return;
		//soonk addition
		//if (UFE.config.characterRotationOptions.rotateOnMoveOnly && myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.idle)) return;
		if (UFE.config.characterRotationOptions.rotateOnMoveOnly
			&& (myMoveSetScript.IsAnimationPlaying("idle") || myMoveSetScript.IsAnimationPlaying("idleP2")
			|| myMoveSetScript.IsAnimationPlaying("rageIdle") || myMoveSetScript.IsAnimationPlaying("rageIdleP2")
			|| myMoveSetScript.IsAnimationPlaying("stunIdle") || myMoveSetScript.IsAnimationPlaying("stunIdleP2"))) return;

		testCharacterRotation(UFE.config.characterRotationOptions._rotationSpeed);
	}



	public void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs
	)
	{
		if (opponent == null) return;

		// Apply Training / Challenge Mode Options
		if (!isAssist)
		{
			if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
				&& ((playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Refill)
				|| (playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Refill)))
			{
				if (!UFE.FindDelaySynchronizedAction(this.RefillLife))
					UFE.DelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime);
			}

			if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
				&& ((playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Refill)
				|| (playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Refill)))
			{
				if (!UFE.FindDelaySynchronizedAction(this.RefillGauge))
					UFE.DelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime);
			}

			if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
				&& currentGaugesPoints[0] < myInfo.maxGaugePoints
				&& ((playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Infinite)
				|| (playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Infinite))) RefillGauge();

			if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
				&& currentLifePoints < myInfo.lifePoints
				&& ((playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Infinite)
				|| (playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Infinite))) RefillLife();
		}


		//Update Hitboxes Position Map
		myHitBoxesScript.UpdateMap(myMoveSetScript.GetCurrentClipFrame(myHitBoxesScript.bakeSpeed));


		// Resolve move
		resolveMove();


		// Check inputs
		//soonk addition
		//if (!isAssist) translateInputs(previousInputs, currentInputs);
		if (!isAssist && !isDead) translateInputs(previousInputs, currentInputs);


		// Gauge Drain
		if (gaugeDPS != 0)
		{
			owner.currentGaugesPoints[(int)gaugeDrainId] -= ((owner.myInfo.maxGaugePoints * (gaugeDPS / 100)) / UFE.config.fps) * UFE.timeScale;
			currentDrained += (gaugeDPS / UFE.config.fps) * UFE.timeScale;
			if (totalDrain != 0 && (owner.currentGaugesPoints[(int)gaugeDrainId] <= 0 || currentDrained >= totalDrain))
			{
				ResetDrainStatus(false, (int)gaugeDrainId);
				//soonk addition
				inRage = false;
				if (myInfo.playerNum == 1)
				{
					defaultBattleGUI.p1Rage = false;
				}
				else
				{
					defaultBattleGUI.p2Rage = false;
				}
			}
		}

		if (firstFinisherPlayed && currentMove != null && currentMove.finisher)
		{
			if (currentMove.currentFrame == currentMove.totalFrames - 1)
			{
				finisherPlayer.loopPointReached += FinisherPlayer_loopPointReached;
			}
		}

		//soonk addition
		// sets armored hit color change
		if (currentMove != null && currentMove.armorOptions.hitsTaken > 0)
		{
			Material currentMat = transform.GetComponentInChildren<SpriteRenderer>().material;
			if (armorFlash > armorColorStart) armorFlash -= armorHitFlashFade;
			armorColor = currentMat.GetColor("_EmissionColor") * (armorFlash);
			currentMat.SetVector("_EmissionColor", armorColor);
		}

		if (sakataReady && currentMove != null && currentMove.moveConnected)
		{
			if (currentMove.currentFrameData == CurrentFrameData.RecoveryFrames)
			{
				sakataReady = false;
				SmokeBomb(myInfo.playerSide);
			}
		}

		if (smokeBombHit)
		{
			smokeBombHit = false;
			if (currentMove == null && isBlocking)
			{
				UFE.DelaySynchronizedAction(UFE._StartLoadingSakata, .5);
			}
			else
			{
				Debug.LogError("GAME OVER");
			}
		}


		// Input Viewer
		string inputDebugger = "";
		if (!isAssist)
		{
			List<InputReferences> inputList = new List<InputReferences>();
			Texture2D lastIconAdded = null;
			foreach (InputReferences inputRef in currentInputs.Keys)
			{
				if (debugger != null && UFE.config.debugOptions.debugMode && debugInfo.inputs)
				{
					inputDebugger += inputRef.inputButtonName + " - " + inputHeldDown[inputRef.engineRelatedButton] + " (" + currentInputs[inputRef].axisRaw + ")\n";
				}
				if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime)
				{
					if (lastIconAdded != inputRef.activeIcon)
					{
						inputList.Add(inputRef);
						UFE.FireButton(inputRef.engineRelatedButton, this);
						lastIconAdded = inputRef.activeIcon;
					}
				}
			}
			UFE.CastInput(inputList.ToArray(), playerNum);
		}


		// Apply Root Motion
		if (applyRootMotion)
		{
			FPVector newPosition = worldTransform.position;
			if (myMoveSetScript.animationPaused)
			{
				if (!lockXMotion) newPosition.x += myHitBoxesScript.GetDeltaPosition().x * myMoveSetScript.GetAnimationSpeed() * UFE.timeScale;
				if (!lockYMotion) newPosition.y += myHitBoxesScript.GetDeltaPosition().y * myMoveSetScript.GetAnimationSpeed() * UFE.timeScale;
				if (!lockZMotion) newPosition.z += myHitBoxesScript.GetDeltaPosition().z * myMoveSetScript.GetAnimationSpeed() * UFE.timeScale;
			}
			else
			{
				if (!lockXMotion) newPosition.x += myHitBoxesScript.GetDeltaPosition().x * UFE.timeScale;
				if (!lockYMotion) newPosition.y += myHitBoxesScript.GetDeltaPosition().y * UFE.timeScale;
				if (!lockZMotion) newPosition.z += myHitBoxesScript.GetDeltaPosition().z * UFE.timeScale;
			}
			worldTransform.position = newPosition;
		}
		else
		{
			if (UFE.config.lockZAxis && !isAssist)
			{
				FPVector newPosition = worldTransform.position;
				newPosition.z = 0;
				worldTransform.position = newPosition;
			}
			localTransform.position = new FPVector(0, 0, 0);
		}

		// Force stand state
		if (!myPhysicsScript.freeze
			//&& !isDead
			&& currentSubState != SubStates.Stunned
			&& introPlayed
			&& myPhysicsScript.IsGrounded()
			&& !myPhysicsScript.IsMoving()
			&& currentMove == null
			&& !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.idle)
			&& !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.idleP2)
			&& !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.rageIdle)
			&& !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.rageIdleP2)
			&& !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.stunIdle)
			&& !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.stunIdleP2)
			&& !myMoveSetScript.IsAnimationPlaying("fallStraight")
			&& isAxisRested(currentInputs)
			&& !myPhysicsScript.isTakingOff
			&& !myPhysicsScript.isLanding
			&& !blockStunned
			&& currentState != PossibleStates.Crouch
			&& !isBlocking
			)
		{


			if (!duckToStand && wasDucking)
			{
				if (currentSide == PossibleSides.P1)
				{
					Timing.RunCoroutine(_DuckToStandP1(), "duckToStandP1");
				}
				else
				{
					Timing.RunCoroutine(_DuckToStandP2(), "duckToStandP2");
				}
			}
			else if (currentState != PossibleStates.StandUp && !outroPlayed)
			{

				//soonk - use this to have airborne kills die right after they finish the stand up
				if (tempDead)
				{
					tempDead = false;
					isDead = true;
					currentLifePoints = 0;
				}


				if (isDead)
				{
					if (!hitWhileDead && !timeOutDead)
					{
						if (currentSide == PossibleSides.P1)
						{
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.stunIdle);
						}
						else
						{
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.stunIdleP2);
						}
					}
				}
				else
				{
					if (firstFinisherPlayed)
					{
						//finisherPlayer.loopPointReached += FinisherPlayer_loopPointReached;

					}
					else
					{
						if (inRage)
						{
							if (currentSide == PossibleSides.P1)
							{
								myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.rageIdle);
							}
							else
							{
								myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.rageIdleP2);
							}
						}
						else
						{
							if (currentSide == PossibleSides.P1)
							{
								myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
							}
							else
							{
								myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idleP2);
							}
						}
					}

				}
			}

			if (currentState == PossibleStates.StandUp)
			{
				currentState = PossibleStates.StandUp;
			}
			else
			{
				currentState = PossibleStates.Stand;
				projCasted = false;
			}

			//myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
			//currentState = PossibleStates.Stand;
			currentSubState = SubStates.Resting;
			if (UFE.config.blockOptions.blockType == BlockType.AutoBlock
				&& myMoveSetScript.basicMoves.blockEnabled) potentialBlock = true;
		}

		if ((myMoveSetScript.IsAnimationPlaying("idle") || myMoveSetScript.IsAnimationPlaying("idleP2")
			|| myMoveSetScript.IsAnimationPlaying("stunIdle") || myMoveSetScript.IsAnimationPlaying("stunIdleP2"))
			&& !UFE.config.lockInputs
			&& !UFE.config.lockMovements
			&& !myPhysicsScript.freeze)
		{
			afkTimer += UFE.fixedDeltaTime;
			if (afkTimer >= myMoveSetScript.basicMoves.idle._restingClipInterval)
			{
				afkTimer = 0;
				int clipNum = FPRandom.Range(2, 6);
				if (myMoveSetScript.AnimationExists("idle_" + clipNum))
				{
					myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle, "idle_" + clipNum, false);
				}
			}
		}
		else
		{
			afkTimer = 0;
		}

		/*
        if (myMoveSetScript.IsAnimationPlaying("idle")
            && !UFE.config.lockInputs 
		    && !UFE.config.lockMovements
            && !myPhysicsScript.freeze) {
            afkTimer += UFE.fixedDeltaTime;
            if (afkTimer >= myMoveSetScript.basicMoves.idle._restingClipInterval) {
                afkTimer = 0;
                int clipNum = FPRandom.Range(2, 6);
                if (myMoveSetScript.AnimationExists("idle_" + clipNum)) {
                    myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle, "idle_" + clipNum, false);
                }
            }
        } else {
            afkTimer = 0;
        }
        */

		// Character colliders based on collision mass and body colliders
		normalizedDistance = FPMath.Clamp(FPVector.Distance(opControlsScript.worldTransform.position, worldTransform.position) / UFE.config.cameraOptions._maxDistance, 0, 1);
		pushOpponentsAway(opControlsScript);
		if (!isAssist) foreach (ControlsScript assist in opControlsScript.assists) pushOpponentsAway(assist);


		// Shake character
		if (shakeDensity > 0)
		{
			shakeDensity -= UFE.fixedDeltaTime;
			if (myHitBoxesScript.isHit && myPhysicsScript.freeze)
			{
				if (shakeCharacter) shake();
			}
		}
		else if (shakeDensity < 0)
		{
			shakeDensity = 0;
			shakeCharacter = false;
		}


		// Shake camera
		if (!isAssist)
		{
			if (shakeCameraDensity > 0)
			{
				shakeCameraDensity -= UFE.fixedDeltaTime * 3;
				if (shakeCamera) shakeCam();
				if (UFE.config.groundBounceOptions.shakeCamOnBounce && myPhysicsScript.isGroundBouncing) shakeCam();
				if (UFE.config.wallBounceOptions.shakeCamOnBounce && myPhysicsScript.isWallBouncing) shakeCam();
			}
			else if (shakeCameraDensity < 0)
			{
				shakeCameraDensity = 0;
				shakeCamera = false;
			}
		}


		// Validate Parry
		if (!isAssist && potentialParry > 0)
		{
			potentialParry -= UFE.fixedDeltaTime;
			if (potentialParry <= 0) potentialParry = 0;
		}


		// Update head movement
		if (headLookScript != null && opControlsScript.HitBoxes != null)
			headLookScript.target = opControlsScript.HitBoxes.GetTransformPosition(myInfo.headLook.target);


		// Execute Move
		if (currentMove != null && currentState != PossibleStates.StandUp)
		{
			ReadMove(currentMove);
		}
		//soonk - addition - standup smoke thing for sakata
		if (myInfo.characterName == "Sakata" && currentState == PossibleStates.StandUp && !projCasted)
		{
			currentMove = myMoveSetScript.standSmoke;
			ReadMoveSakataStandUp(currentMove);
		}

		// Validate rotation
		validateRotation();


		// Apply Stun
		if ((currentSubState == SubStates.Stunned || blockStunned) && stunTime > 0 && !myPhysicsScript.freeze) // && !isDead)
		{
			ApplyStun(previousInputs, currentInputs);
		}

		//soonk addition
		// Handle StandUp stuff
		if (currentState == PossibleStates.StandUp)
		{
			if (standUpTimeTemp > 0)
			{
				standUpTimeTemp -= Time.fixedDeltaTime;
			}
			else
			{
				currentState = PossibleStates.Stand;

			}
		}

		// Apply Forces
		if (GetActive()) myPhysicsScript.ApplyForces(currentMove);


		// Intro and Enter Moves
		if (!introPlayed && (isAssist || myMoveSetScript.intro == null))
		{
			introPlayed = true;
			if (!isAssist && playerNum == 2 && opControlsScript.introPlayed)
			{
				UFE.CastNewRound(2);
			}
			else if (isAssist && enterMove != null)
			{
				CastMove(enterMove, true);
				enterMove = null;
			}
			//soonk addition (and removal)
		}
		else if ((gameObject.name == "Player1" && !introPlayed && currentMove == null) ||
				 (gameObject.name == "Player2" && !introPlayed && currentMove == null))
		{
			KillCurrentMove();
			if (playerNum == 1)
			{
				CastMove(myMoveSetScript.intro, true, true, false);
			}
			else
			{
				CastMove(myMoveSetScript.introP2, true, true, false);
			}
			if (currentMove == null)
			{
				introPlayed = true;
				UFE.CastNewRound(2);
			}
		}
		/*
        } else if (currentMove == null && ((playerNum == 1 && !introPlayed) || 
                  (playerNum == 2 && !introPlayed && opControlsScript.introPlayed))) {
            //soonk addition
            KillCurrentMove();
            if (playerNum == 1)
            {
                CastMove(myMoveSetScript.intro, true, true, false);
            } else
            {
                CastMove(myMoveSetScript.introP2, true, true, false);
            }
            if (currentMove == null)
            {
                introPlayed = true;
                UFE.CastNewRound(2);
            }
        }
        */

		// Assist - Play Exit Move after Enter Move
		if (stunTime == 0 && currentState == PossibleStates.Stand && currentMove == null && enterMove == null && exitMove != null)
		{
			CastMove(exitMove, true);
		}

		//soonk addition
		/*
        //Finisher test
        if (isDead && opControlsScript.currentMove != null && opControlsScript.currentMove.finisher)
        {
            if (!firstFinisherStarted)
            {
                firstFinisherStarted = true;
                Timing.KillCoroutines("Finisher");
                Timing.KillCoroutines("Countdown");
                defaultBattleGUI.FinishingTransition();
            }
            if ((!firstFinisherPlayed || !opControlsScript.firstFinisherPlayed) && (finisherReady || opControlsScript.finisherReady))
            {
                firstFinisherPlayed = true;
                opControlsScript.firstFinisherPlayed = true;
                finisherPlayer = UFE.overlayCam.GetComponent<VideoPlayer>();
                finisherPlayer.Play();
                opControlsScript.currentMove.currentTick = 1;
            }
        }
        */
		//Finisher test
		if (isDead && opControlsScript.currentMove != null && opControlsScript.currentMove.finisher)
		{
			if (!firstFinisherStarted)
			{
				defaultBattleGUI.stopSound();
				firstFinisherStarted = true;
				Timing.KillCoroutines("Finisher");
				Timing.KillCoroutines("Countdown");
				defaultBattleGUI.FinishingTransition();
			}
			if ((!firstFinisherPlayed || !opControlsScript.firstFinisherPlayed) && (finisherReady || opControlsScript.finisherReady))
			{
				firstFinisherPlayed = true;
				opControlsScript.firstFinisherPlayed = true;
				finisherPlayer = UFE.overlayCam.GetComponent<VideoPlayer>();

				finisherPlayer.Play();
				Debug.Log("Play Finisher! " + Time.frameCount);
				opControlsScript.currentMove.currentTick = 1;
				UFE.cameraEffects.Reset();
			}

		}
		// Test Current Challenge
		if (!isAssist && challengeMode != null && challengeMode.complete)
		{
			UFE.FireAlert("Success", this); // TODO
			if (challengeMode.moveToNext)
			{
				UFE.DelaySynchronizedAction(this.StartNextChallenge, .6);
			}
			else
			{
				UFE.DelaySynchronizedAction(UFE.fluxCapacitor.EndRound, (Fix64)5);
			}
			challengeMode.Stop();
		}


		// Update Unity Transforms with Fixed Point Transforms
		transform.position = worldTransform.position.ToVector();
		character.transform.localPosition = localTransform.position.ToVector();
		character.transform.rotation = localTransform.rotation.ToQuaternion();


		// Run Debugger
		if (!isAssist && debugger != null && UFE.config.debugOptions.debugMode)
		{
			debugger.text = "";
			if (UFE.config.debugOptions.debugMode &&
				(!UFE.config.debugOptions.trainingModeDebugger || UFE.gameMode == GameMode.TrainingRoom))
			{
				debugger.text += "FPS: " + (1.0f / UFE.fixedDeltaTime) + "\n";
				debugger.text += "-----Character Info-----\n";
				if (debugInfo.lifePoints) debugger.text += "Life Points: " + currentLifePoints + "\n";
				if (debugInfo.gaugePoints) debugger.text += "Gauge Points: " + currentGaugesPoints[0] + "\n";
				if (debugInfo.position) debugger.text += "Position: " + worldTransform.position + "\n";
				if (debugInfo.currentState) debugger.text += "State: " + currentState + "\n";
				if (debugInfo.currentState) debugger.text += "Taking Off: " + myPhysicsScript.isTakingOff + "\n";
				if (debugInfo.currentSubState) debugger.text += "Sub State: " + currentSubState + "\n";
				if (debugInfo.currentState) debugger.text += "Potential Block: " + potentialBlock + "\n";
				if (debugInfo.currentState) debugger.text += "Is Blocking: " + isBlocking + "\n";
				if (debugInfo.stunTime && stunTime > 0) debugger.text += "Stun Time: " + stunTime + "\n";
				if (opControlsScript != null && opControlsScript.comboHits > 0)
				{
					debugger.text += "Current Combo\n";
					if (debugInfo.comboHits) debugger.text += "- Total Hits: " + opControlsScript.comboHits + "\n";
					if (debugInfo.comboDamage)
					{
						debugger.text += "- Total Damage: " + opControlsScript.comboDamage + "\n";
						debugger.text += "- Hit Damage: " + opControlsScript.comboHitDamage + "\n";
					}
				}

				// Other uses
				if (potentialParry > 0) debugger.text += "Parry Window: " + potentialParry + "\n";
				//debugger.text += "Air Jumps: "+ myPhysicsScript.currentAirJumps + "\n";
				//debugger.text += "Horizontal Force: "+ myPhysicsScript.horizontalForce + "\n";
				//debugger.text += "Vertical Force: "+ myPhysicsScript.verticalForce + "\n";

				if (UFE.config.debugOptions.p1DebugInfo.currentMove && currentMove != null)
				{
					debugger.text += "-----Move Info-----\n";
					debugger.text += "Move: " + currentMove.name + "\n";
					debugger.text += "Frames: " + currentMove.currentFrame + "/" + currentMove.totalFrames + "\n";
					debugger.text += "Tick: " + currentMove.currentTick + "\n";
					debugger.text += "Animation Speed: " + myMoveSetScript.GetAnimationSpeed() + "\n";
				}
			}
			if (inputDebugger != "") debugger.text += inputDebugger;
			if (aiDebugger != null && debugInfo.aiWeightList) debugger.text += aiDebugger;
		}
	}

	private void FinisherPlayer_loopPointReached(VideoPlayer source)
	{
		if (!finisherEnded)
		{
			print("finisher ended");
			UFE.DelaySynchronizedAction(this.TakeEndScreenshot, .1f);
			finisherEnded = true;
			Debug.Log("Video Finisher Ended " + Time.frameCount);
			Timing.KillAllCoroutines();
			UFE.config.roundOptions._endGameDelay = .1f;
			this.EndRound();
			UFE.fluxCapacitor.EndRound();
		}
	}

	private void pushOpponentsAway(ControlsScript opControlsScript)
	{
		if (ignoreCollisionMass || opControlsScript.ignoreCollisionMass || opControlsScript == null || opControlsScript.HitBoxes == null) return;

		Fix64 pushForce = myHitBoxesScript.TestCollision(worldTransform.position, opControlsScript.worldTransform.position, opControlsScript.HitBoxes.hitBoxes);
		if (pushForce > 0)
		{
			if (worldTransform.position.x < opControlsScript.worldTransform.position.x)
			{
				worldTransform.Translate(new FPVector(-.1 * pushForce, 0, 0));
			}
			else
			{
				worldTransform.Translate(new FPVector(.1 * pushForce, 0, 0));
			}
			if (opControlsScript.worldTransform.position.x >= UFE.config.selectedStage._rightBoundary)
			{
				opControlsScript.worldTransform.Translate(new FPVector(-.1 * pushForce, 0, 0));
			}
		}

		pushForce = myInfo.physics._groundCollisionMass - FPVector.Distance(opControlsScript.worldTransform.position, worldTransform.position);
		if (pushForce > 0)
		{
			if (worldTransform.position.x < opControlsScript.worldTransform.position.x)
			{
				worldTransform.Translate(new FPVector(-.5 * pushForce, 0, 0));
			}
			else
			{
				worldTransform.Translate(new FPVector(.5 * pushForce, 0, 0));
			}
			if (opControlsScript.worldTransform.position.x >= UFE.config.selectedStage._rightBoundary)
			{
				opControlsScript.worldTransform.Translate(new FPVector(-.5 * pushForce, 0, 0));
			}
		}
	}

	private bool testMoveExecution(ButtonPress buttonPress)
	{
		return testMoveExecution(new ButtonPress[] { buttonPress });
	}

	private bool testMoveExecution(ButtonPress[] buttonPresses)
	{
		MoveInfo tempMove = myMoveSetScript.GetMove(buttonPresses, 0, currentMove, false);
		if (tempMove != null)
		{
			storedMove = tempMove;
			storedMoveTime = (UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
			return true;
		}
		return false;
	}

	private void resolveMove()
	{
		if (myPhysicsScript.freeze) return;
		if (storedMoveTime > 0) storedMoveTime -= UFE.fixedDeltaTime;

		//soonk - fix for kyla stuff
		if (storedMove != null && moveOnConnect != null && storedMove == moveOnConnect)
		{
			KillCurrentMove();
			CastMove(moveOnConnect, true);
			storedMove = null;
			storedMoveTime = 0;
		}

		if (storedMoveTime <= 0 && storedMove != null)
		{
			storedMoveTime = 0;
			if (UFE.config.executionBufferType != ExecutionBufferType.NoBuffer) storedMove = null;
		}
		/*
        if (currentMove != null && storedMove == null && !opControlsScript.isDead)
        {
            storedMove = myMoveSetScript.GetNextMove(currentMove);
        }
        */
		if (currentMove != null && storedMove == null && (!opControlsScript.isDead || currentMove.finisher))
			storedMove = myMoveSetScript.GetNextMove(currentMove);

		if (overrideFinsiher != null)
		{
			KillCurrentMove();
			this.SetMove(overrideFinsiher);
			storedMove = null;
			storedMoveTime = 0;
			overrideFinsiher = null;
		}

		if (storedMove != null && (currentMove == null || myMoveSetScript.SearchMove(storedMove.moveName, currentMove.frameLinks)))
		{
			bool confirmQueue = false;
			bool ignoreConditions = false;
			if (currentMove != null && UFE.config.executionBufferType == ExecutionBufferType.OnlyMoveLinks)
			{
				foreach (FrameLink frameLink in currentMove.frameLinks)
				{
					if (frameLink.cancelable)
					{
						confirmQueue = true;
					}

					if (frameLink.ignorePlayerConditions)
					{
						ignoreConditions = true;
					}

					if (confirmQueue)
					{
						foreach (MoveInfo move in frameLink.linkableMoves)
						{
							if (storedMove.name == move.name)
							{
								opControlsScript.ncfa_active = false;
								storedMove.overrideStartupFrame = frameLink.nextMoveStartupFrame - 1;
							}
						}
					}
				}
			}
			else if (UFE.config.executionBufferType == ExecutionBufferType.AnyMove
					|| (currentMove == null
						&& storedMoveTime >= ((Fix64)(UFE.config.executionBufferTime - 2) / (Fix64)UFE.config.fps)))
			{
				confirmQueue = true;
			}

			if (confirmQueue && (ignoreConditions || myMoveSetScript.ValidateMoveStances(storedMove.selfConditions, this)))
			{
				//soonk addition
				//fix for jump buffering
				if (myMoveSetScript.totalAirMoves == 1 && (myPhysicsScript.IsJumping() || myPhysicsScript.isLanding))
				{
					storedMoveTime += Time.fixedDeltaTime;
				}
				//fix so finishing moves don't play until the last frame
				else if (storedMove.finisher)
				{
					if (!firstFinisherPlayed)
					{
						KillCurrentMove();
						this.SetMove(storedMove);
						storedMove = null;
						storedMoveTime = 0;
					}
					else
					{
						if (currentMove.currentFrame == currentMove.totalFrames - 1)
						{
							KillCurrentMove();
							this.SetMove(storedMove);
							storedMove = null;
							storedMoveTime = 0;
						}
						else
						{
							storedMoveTime += Time.fixedDeltaTime;
						}
					}
					// Finisher achievement tracking
					if (playerNum == 1)
					{
						if (currentMove.finisherStages == FinisherStages.NM1)
						{
							UFE.p1InGameSaveInfo.charFinishers[myInfo.characterName + "_nm1"] = true;
							finisher1 = true;
						}
						if (currentMove.finisherStages == FinisherStages.NM2)
						{
							UFE.p1InGameSaveInfo.charFinishers[myInfo.characterName + "_nm2"] = true;
							finisher2 = true;
						}
						if (currentMove.finisherStages == FinisherStages.NM3)
						{
							UFE.p1InGameSaveInfo.charFinishers[myInfo.characterName + "_nm3"] = true;
							finisher3 = true;
						}
						if (finisher1 && finisher2 && finisher3)
						{
							CharacterCompleteFinishersP1(myInfo.characterName);
						}
						CharacterSingleFinishersP1(myInfo.characterName);
					}
					else
					{
						if (currentMove.finisherStages == FinisherStages.NM1)
						{
							UFE.p2InGameSaveInfo.charFinishers[myInfo.characterName + "_nm1"] = true;
							finisher1 = true;
						}
						if (currentMove.finisherStages == FinisherStages.NM2)
						{
							UFE.p2InGameSaveInfo.charFinishers[myInfo.characterName + "_nm2"] = true;
							finisher2 = true;
						}
						if (currentMove.finisherStages == FinisherStages.NM3)
						{
							UFE.p2InGameSaveInfo.charFinishers[myInfo.characterName + "_nm3"] = true;
							finisher3 = true;
						}
						if (finisher1 && finisher2 && finisher3)
						{
							//CharacterCompleteFinishersP2(myInfo.characterName);
						}
					}
				}
				else
				{
					KillCurrentMove();
					this.SetMove(storedMove);
					//special move check for sakata
					if (playerNum == 1)
					{
						if (UFE.p1InGameSaveInfo.charMoves.ContainsKey(myInfo.characterName + "_" + storedMove.name))
						{
							UFE.p1InGameSaveInfo.charMoves[myInfo.characterName + "_" + storedMove.name] = true;
							specialsDone++;

							/*
                            // sakata testing build stuff
                            if (myInfo.characterName != "Sakata" && opInfo.characterName != "Sakata")
                            {
                                sakataReady = true;
                            }
                            */

							//Sakata loading stuff
							if (specialsDone > 5 && UFE.sakataIncluded && myInfo.characterName != "Sakata" && opInfo.characterName != "Sakata" && opInfo.characterName != "MindMaster")
							{
								sakataReady = true;
							}

						}
					}
					else
					{
						if (UFE.p2InGameSaveInfo.charMoves.ContainsKey(myInfo.characterName + "_" + storedMove.name))
						{
							UFE.p2InGameSaveInfo.charMoves[myInfo.characterName + "_" + storedMove.name] = true;
							specialsDone++;
							//Sakata loading stuff
							if (specialsDone > 5 && UFE.sakataIncluded && myInfo.characterName != "Sakata" && opInfo.characterName != "Sakata")
							{
								sakataReady = true;
							}
						}
					}



					storedMove = null;
					storedMoveTime = 0;
				}
			}
		}
	}

	void SmokeBomb(int pSide)
	{
		float spawnSide;
		Vector3 pPos = this.transform.position;
		spawnSide = pSide == 1 ? (float)UFE.config.selectedStage._rightBoundary : (float)UFE.config.selectedStage._leftBoundary;
		GameObject smokeBomb = Instantiate(UFE.config.roundOptions.sakataSmoke, new Vector3(spawnSide * 2, 5, 1), new Quaternion(0, 0, 0, 0));
		SmokeBombMovement smokeBombScript = smokeBomb.AddComponent<SmokeBombMovement>();
		smokeBombScript.startPos = smokeBomb.transform.position;
		smokeBombScript.endPos = pPos;
		smokeBombScript.playerName = myInfo.characterName;
	}

	private void translateInputs(
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs
	)
	{
		if (!introPlayed || !opControlsScript.introPlayed) return;
		if (UFE.config.lockInputs && !UFE.config.roundOptions.allowMovementStart) return;
		if (UFE.config.lockMovements) return;
		if (tempDead) return;

		foreach (InputReferences inputRef in currentInputs.Keys)
		{
			InputEvents ev = currentInputs[inputRef];
			//if (myInfo.customControls.enabled && myInfo.customControls.overrideInputs) inputRef.engineRelatedButton = characterInputOverride(inputRef.engineRelatedButton);

			if (((inputRef.engineRelatedButton == ButtonPress.Down && ev.axisRaw >= 0)
				|| (inputRef.engineRelatedButton == ButtonPress.Up && ev.axisRaw <= 0))
				&& myPhysicsScript.IsGrounded()
				&& !myHitBoxesScript.isHit

				&& currentSubState != SubStates.Stunned
				&& currentState != PossibleStates.StandUp)
			{
				currentState = PossibleStates.Stand;
			}

			// On Axis Release
			if (inputRef.inputType != InputType.Button && inputHeldDown[inputRef.engineRelatedButton] > 0 && ev.axisRaw == 0)
			{
				if ((inputRef.engineRelatedButton == ButtonPress.Back && UFE.config.blockOptions.blockType == BlockType.HoldBack))
				{
					potentialBlock = false;
				}
				downHeld = false;
				// Pressure Sensitive Jump
				if (myInfo.physics.pressureSensitiveJump
					&& myPhysicsScript.IsGrounded()
					&& myPhysicsScript.isTakingOff
					&& !myPhysicsScript.IsJumping()
					&& inputRef.engineRelatedButton == ButtonPress.Up)
				{
					UFE.FindAndRemoveDelaySynchronizedAction(myPhysicsScript.Jump);

					Fix64 jumpDelaySeconds = (Fix64)myInfo.physics.jumpDelay / (Fix64)UFE.config.fps;
					Fix64 pressurePercentage = FPMath.Min(inputHeldDown[inputRef.engineRelatedButton] / jumpDelaySeconds, 1);
					Fix64 newJumpForce = FPMath.Max((myInfo.physics._jumpForce * pressurePercentage), myInfo.physics._minJumpForce);
					if (newJumpForce < myInfo.physics.minJumpDelay) newJumpForce = myInfo.physics.minJumpDelay;

					myPhysicsScript.Jump(newJumpForce);

					//Debug.Log((inputHeldDown[inputRef.engineRelatedButton] * UFE.config.fps) + " - " + pressurePercentage + "% (" + (UFE.ToDouble(myInfo.physics.jumpForce) * pressurePercentage) + ")");
				}

				// Move Execution
				MoveInfo tempMove = myMoveSetScript.GetMove(new ButtonPress[] { inputRef.engineRelatedButton }, inputHeldDown[inputRef.engineRelatedButton], currentMove, true);
				inputHeldDown[inputRef.engineRelatedButton] = 0;
				if (tempMove != null)
				{
					storedMove = tempMove;
					storedMoveTime = ((Fix64)UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
					return;
				}
			}

			if (inputHeldDown[inputRef.engineRelatedButton] == 0 && inputRef.inputType != InputType.Button)
			{
				inputRef.activeIcon = ev.axisRaw > 0 ? inputRef.inputViewerIcon1 : inputRef.inputViewerIcon2;
			}

			/*if (inputController.GetButtonUp(inputRef)) {
				storedMove = myMoveSetScript.GetMove(new ButtonPress[]{inputRef.engineRelatedButton}, inputHeldDown[inputRef.engineRelatedButton], currentMove, true);
				inputHeldDown[inputRef.engineRelatedButton] = 0;
				if (storedMove != null){
					storedMoveTime = ((float)UFE.config.executionBufferTime / UFE.config.fps);
					return;
				}
			}*/

			// On Axis Press
			if (inputRef.inputType != InputType.Button && ev.axisRaw != 0)
			{
				if (inputRef.inputType == InputType.HorizontalAxis)
				{
					// Horizontal Movements
					if (ev.axisRaw > 0)
					{
						if (mirror == 1)
						{
							inputHeldDown[ButtonPress.Forward] = 0;
							inputRef.engineRelatedButton = ButtonPress.Back;
						}
						else
						{
							inputHeldDown[ButtonPress.Back] = 0;
							inputRef.engineRelatedButton = ButtonPress.Forward;
						}
						inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;
						if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton)) return;

						if (currentState == PossibleStates.Stand
							&& !isBlocking
							&& !myPhysicsScript.isTakingOff
							&& !myPhysicsScript.isLanding
							&& currentSubState != SubStates.Stunned
							&& !blockStunned
							&& currentMove == null
							//soonk - stand up stuff
							&& currentState != PossibleStates.StandUp
							&& myMoveSetScript.basicMoves.moveEnabled)
						{
							CancelDTS();
							myPhysicsScript.Move(-mirror, ev.axisRaw);
						}
					}

					if (ev.axisRaw < 0)
					{
						if (mirror == 1)
						{
							inputHeldDown[ButtonPress.Back] = 0;
							inputRef.engineRelatedButton = ButtonPress.Forward;
						}
						else
						{
							inputHeldDown[ButtonPress.Forward] = 0;
							inputRef.engineRelatedButton = ButtonPress.Back;
						}
						//inputRef.engineRelatedButton = mirror == 1? ButtonPress.Foward : ButtonPress.Back;
						inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;
						if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton)) return;

						if (currentState == PossibleStates.Stand
							&& !isBlocking
							&& !myPhysicsScript.isTakingOff
							&& !myPhysicsScript.isLanding
							&& currentSubState != SubStates.Stunned
							&& !blockStunned
							&& currentMove == null
							//soonk - stand up stuff
							&& currentState != PossibleStates.StandUp
							&& myMoveSetScript.basicMoves.moveEnabled)
						{
							CancelDTS();
							myPhysicsScript.Move(mirror, ev.axisRaw);
						}
					}

					// Check for potential blocking
					if (inputRef.engineRelatedButton == ButtonPress.Back
						&& UFE.config.blockOptions.blockType == BlockType.HoldBack
						&& !myPhysicsScript.isTakingOff
						&& myMoveSetScript.basicMoves.blockEnabled)
					{
						potentialBlock = true;
					}

					// Check for potential parry
					if (((inputRef.engineRelatedButton == ButtonPress.Back && UFE.config.blockOptions.parryType == ParryType.TapBack) ||
						 (inputRef.engineRelatedButton == ButtonPress.Forward && UFE.config.blockOptions.parryType == ParryType.TapForward))
						&& (potentialParry == 0 || UFE.config.blockOptions.easyParry)
						&& inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime
						&& currentMove == null
						&& !isBlocking
						&& !myPhysicsScript.isTakingOff
						&& currentSubState != SubStates.Stunned
						&& !blockStunned
						&& myMoveSetScript.basicMoves.parryEnabled)
					{
						potentialParry = UFE.config.blockOptions._parryTiming;
					}


				}
				else
				{
					// Vertical Movements
					if (ev.axisRaw > 0)
					{
						inputRef.engineRelatedButton = ButtonPress.Up;
						if (!myPhysicsScript.isTakingOff && !myPhysicsScript.isLanding)
						{
							if (inputHeldDown[inputRef.engineRelatedButton] == 0)
							{
								if (!myPhysicsScript.IsGrounded() && myInfo.physics.canJump && myInfo.physics.multiJumps > 1)
								{
									myPhysicsScript.Jump();
								}
								if (testMoveExecution(inputRef.engineRelatedButton)) return;
							}

							if (!myPhysicsScript.freeze
								&& !myPhysicsScript.IsJumping()
								&& storedMove == null
								&& currentMove == null
								&& currentState == PossibleStates.Stand
								&& currentSubState != SubStates.Stunned
								&& !isBlocking
								&& myInfo.physics.canJump
								&& !blockStunned
								//soonk - stand up stuff
								&& currentState != PossibleStates.StandUp
								&& myMoveSetScript.basicMoves.jumpEnabled)
							{

								myPhysicsScript.isTakingOff = true;
								potentialBlock = false;
								potentialParry = 0;

								Fix64 jumpDelaySeconds = (Fix64)myInfo.physics.jumpDelay / (Fix64)UFE.config.fps;
								UFE.DelaySynchronizedAction(myPhysicsScript.Jump, jumpDelaySeconds);

								if (currentSide == PossibleSides.P1)
								{
									if (myMoveSetScript.AnimationExists(myMoveSetScript.basicMoves.takeOff.name))
									{
										myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.takeOff);

										if (myMoveSetScript.basicMoves.takeOff.autoSpeed)
										{
											myMoveSetScript.SetAnimationSpeed(
												myMoveSetScript.basicMoves.takeOff.name,
												myMoveSetScript.GetAnimationLength(myMoveSetScript.basicMoves.takeOff.name) / jumpDelaySeconds);
										}

									}
								}
								else
								{
									if (myMoveSetScript.AnimationExists(myMoveSetScript.basicMoves.takeOffP2.name))
									{
										myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.takeOffP2);

										if (myMoveSetScript.basicMoves.takeOffP2.autoSpeed)
										{
											myMoveSetScript.SetAnimationSpeed(
												myMoveSetScript.basicMoves.takeOffP2.name,
												myMoveSetScript.GetAnimationLength(myMoveSetScript.basicMoves.takeOffP2.name) / jumpDelaySeconds);
										}

									}
								}
							}
						}
						inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;

					}
					else if (ev.axisRaw < 0)
					{
						inputRef.engineRelatedButton = ButtonPress.Down;

						inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;
						if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton)) return;

						//soonk - for stand up blocking + removal of unblockables
						if (currentState == PossibleStates.StandUp || (currentState == PossibleStates.Stand && stunTime > 0))
						{
							downHeld = true;
						}
						else
						{
							downHeld = false;
						}

						if (!myPhysicsScript.freeze
							&& myPhysicsScript.IsGrounded()
							&& currentMove == null
							&& currentSubState != SubStates.Stunned
							&& !myPhysicsScript.isTakingOff
							&& !blockStunned
							//soonk - stand up stuff
							&& currentState != PossibleStates.StandUp
							&& myMoveSetScript.basicMoves.crouchEnabled)
						{

							if (currentState == PossibleStates.StandUp)
							{
								standToDuck = true;
								wasDucking = true;
							}
							currentState = PossibleStates.Crouch;
							if (!isBlocking)
							{
								if (!standToDuck)
								{
									if (currentSide == PossibleSides.P1)
									{
										Timing.RunCoroutine(_StandToDuckP1(), "standToDuckP1");
									}
									else
									{
										Timing.RunCoroutine(_StandToDuckP2(), "standToDuckP2");
									}
								}
								else
								{
									if (currentSide == PossibleSides.P1)
									{
										myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.crouching, false);
									}
									else
									{
										myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.crouchingP2, false);
									}

								}
							}
							else
							{

								if (currentSide == PossibleSides.P1)
								{
									myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingCrouchingPose, false);
								}
								else
								{
									myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingCrouchingPoseP2, false);
								}
							}
						}
					}
				}

				// Axis + Button Execution
				foreach (InputReferences inputRef2 in currentInputs.Keys)
				{
					InputEvents ev2 = currentInputs[inputRef2];
					InputEvents p2;
					if (!previousInputs.TryGetValue(inputRef2, out p2))
					{
						p2 = InputEvents.Default;
					}
					bool button2Down = ev2.button && !p2.button;

					if (button2Down)
					{
						// If its an axis, attempt diagonal input injection
						if (inputRef2.inputType != InputType.Button)
						{
							ButtonPress newInputRefValue = inputRef.engineRelatedButton;
							if (inputRef2 != inputRef && inputRef2.inputType == InputType.HorizontalAxis)
							{
								ButtonPress b2Press = ButtonPress.Back;
								if ((ev2.axisRaw > 0 && mirror == -1) || (ev2.axisRaw < 0 && mirror == 1))
								{
									b2Press = ButtonPress.Forward;
								}
								else if ((ev2.axisRaw < 0 && mirror == -1) || (ev2.axisRaw > 0 && mirror == 1))
								{
									b2Press = ButtonPress.Back;
								}
								/*
                                if (inputRef.engineRelatedButton == ButtonPress.Down && b2Press == ButtonPress.Back)
                                {
                                    standToDuck = false;
                                    duckToStand = true;
                                    wasDucking = false;
                                }
                                */
								/* diagonal removal 
                                if (inputRef.engineRelatedButton == ButtonPress.Down && b2Press == ButtonPress.Back)
                                {
                                    newInputRefValue = ButtonPress.DownBack;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Up && b2Press == ButtonPress.Back)
                                {
                                    newInputRefValue = ButtonPress.UpBack;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Down && b2Press == ButtonPress.Forward)
                                {
                                    newInputRefValue = ButtonPress.DownForward;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Up && b2Press == ButtonPress.Forward)
                                {
                                    newInputRefValue = ButtonPress.UpForward;
                                }
                                */
							}
							else if (inputRef2 != inputRef && inputRef2.inputType == InputType.VerticalAxis)
							{
								ButtonPress b2Press = ev2.axisRaw > 0 ? ButtonPress.Up : ButtonPress.Down;
								/*
                                if (inputRef.engineRelatedButton == ButtonPress.Back && b2Press == ButtonPress.Down)
                                {
                                    standToDuck = false;
                                    duckToStand = true;
                                    wasDucking = false;
                                }
                                */
								/* diagonal removal 
                                if (inputRef.engineRelatedButton == ButtonPress.Back && b2Press == ButtonPress.Down)
                                {
                                    newInputRefValue = ButtonPress.DownBack;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Forward && b2Press == ButtonPress.Down)
                                {
                                    newInputRefValue = ButtonPress.DownForward;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Back && b2Press == ButtonPress.Up)
                                {
                                    newInputRefValue = ButtonPress.UpBack;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Forward && b2Press == ButtonPress.Up)
                                {
                                    newInputRefValue = ButtonPress.UpForward;
                                }
                                */
							}

							// If the value has changed, send the new axis input
							if (newInputRefValue != inputRef.engineRelatedButton)
							{
								MoveInfo tempMove = myMoveSetScript.GetMove(
									new ButtonPress[] { newInputRefValue }, 0, currentMove, false, false);

								if (tempMove != null)
								{
									storedMove = tempMove;
									storedMoveTime = ((Fix64)UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
									return;
								}
							}
						}
						// If its a button, send both axis and button to attempt double input execution
						else
						{
							MoveInfo tempMove = myMoveSetScript.GetMove(
								new ButtonPress[] { inputRef.engineRelatedButton, inputRef2.engineRelatedButton }, 0, currentMove, false, false);

							if (tempMove != null)
							{
								storedMove = tempMove;
								storedMoveTime = ((Fix64)UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
								return;
							}
						}
					}
				}
			}

			// Button Press
			if (inputRef.inputType == InputType.Button && !UFE.config.lockInputs)
			{
				InputEvents p;
				if (!previousInputs.TryGetValue(inputRef, out p))
				{
					p = InputEvents.Default;
				}
				bool buttonDown = ev.button && !p.button;
				bool buttonUp = !ev.button && p.button;


				if (ev.button)
				{


					if (myMoveSetScript.CompareBlockButtons(inputRef.engineRelatedButton))
					{
						if ((currentSubState != SubStates.Stunned || ncfa_active)
						//&& currentSubState != SubStates.Stunned
						&& !myPhysicsScript.isTakingOff
						&& !blockStunned
						&& myMoveSetScript.basicMoves.blockEnabled)
						{
							potentialBlock = true;
							CheckBlocking(true);
						}
					}

					if (myMoveSetScript.CompareParryButtons(inputRef.engineRelatedButton)
						&& inputHeldDown[inputRef.engineRelatedButton] == 0
						&& potentialParry == 0
						&& currentMove == null
						&& !isBlocking
						&& currentSubState != SubStates.Stunned
						&& !myPhysicsScript.isTakingOff
						&& !blockStunned
						&& myMoveSetScript.basicMoves.parryEnabled)
					{
						potentialParry = UFE.config.blockOptions._parryTiming;
					}

					inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;

					// Plinking
					if (inputHeldDown[inputRef.engineRelatedButton] <= ((Fix64)UFE.config.plinkingDelay / (Fix64)UFE.config.fps))
					{
						foreach (InputReferences inputRef2 in currentInputs.Keys)
						{
							InputEvents ev2 = currentInputs[inputRef2];
							InputEvents p2;
							if (!previousInputs.TryGetValue(inputRef2, out p2))
							{
								p2 = InputEvents.Default;
							}
							bool button2Down = ev2.button && !p2.button;

							if (inputRef2 != inputRef && inputRef2.inputType == InputType.Button && button2Down)
							{
								inputHeldDown[inputRef2.engineRelatedButton] += UFE.fixedDeltaTime;
								MoveInfo tempMove = myMoveSetScript.GetMove(
									new ButtonPress[] { inputRef.engineRelatedButton, inputRef2.engineRelatedButton }, 0, currentMove, false, true);

								if (tempMove != null)
								{
									if (currentMove != null && currentMove.currentFrame <= UFE.config.plinkingDelay) KillCurrentMove();
									storedMove = tempMove;
									storedMoveTime = ((Fix64)UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
									return;
								}
							}
						}
					}
				}


				if (buttonDown)
				{
					//soonk - test
					if (inputRef.engineRelatedButton == ButtonPress.Button5 && UFE.config.blockOptions.blockType == BlockType.HoldButton5)
					{
						blockPressed = true;
					}
					MoveInfo tempMove = myMoveSetScript.GetMove(new ButtonPress[] { inputRef.engineRelatedButton }, 0, currentMove, false);
					if (tempMove != null)
					{
						// If plinking occured and input sequence is higher then the plinked move, override it
						if (storedMove == null || tempMove.defaultInputs.buttonSequence.Length > storedMove.defaultInputs.buttonSequence.Length)
						{
							storedMove = tempMove;
							storedMoveTime = (UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
						}
						return;
					}

				}

				if (buttonUp)
				{
					inputHeldDown[inputRef.engineRelatedButton] = 0;
					MoveInfo tempMove = myMoveSetScript.GetMove(new ButtonPress[] { inputRef.engineRelatedButton }, inputHeldDown[inputRef.engineRelatedButton], currentMove, true);
					if (tempMove != null)
					{
						storedMove = tempMove;
						storedMoveTime = ((Fix64)UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
						return;
					}

					if (myMoveSetScript.CompareBlockButtons(inputRef.engineRelatedButton)
						&& !myPhysicsScript.isTakingOff)
					{
						blockPressed = false;
						potentialBlock = false;
						CheckBlocking(false);
					}
				}
			}
		}
	}




	//soonk addition
	//transition functions
	private IEnumerator<float> _StandToDuckP1()
	{
		//BasicMoveInfo stc = currentSide == PossibleSides.P1 ? myMoveSetScript.basicMoves.standToCrouch : myMoveSetScript.basicMoves.standToCrouchP2;
		BasicMoveInfo stc = myMoveSetScript.basicMoves.standToCrouch;
		while (true)
		{
			myMoveSetScript.PlayBasicMove(stc, false);
			yield return Timing.WaitForSeconds(.1f);
			standToDuck = true;
			duckToStand = false;
			wasDucking = true;
			Timing.KillCoroutines("standToDuckP1");
		}
	}
	private IEnumerator<float> _StandToDuckP2()
	{
		BasicMoveInfo stc = myMoveSetScript.basicMoves.standToCrouchP2;
		while (true)
		{
			myMoveSetScript.PlayBasicMove(stc, false);
			yield return Timing.WaitForSeconds(.1f);
			standToDuck = true;
			duckToStand = false;
			wasDucking = true;
			Timing.KillCoroutines("standToDuckP2");
		}
	}
	private IEnumerator<float> _DuckToStandP1()
	{
		BasicMoveInfo cts = myMoveSetScript.basicMoves.crouchToStand;
		while (true)
		{
			myMoveSetScript.PlayBasicMove(cts, false);
			dtsStarted = true;
			yield return Timing.WaitForSeconds(.1f);
			wasDucking = false;
			duckToStand = true;
			standToDuck = false;
			dtsStarted = false;
			Timing.KillCoroutines("duckToStandP1");
		}
	}
	private IEnumerator<float> _DuckToStandP2()
	{
		BasicMoveInfo cts = myMoveSetScript.basicMoves.crouchToStandP2;
		while (true)
		{
			myMoveSetScript.PlayBasicMove(cts, false);
			dtsStarted = true;
			yield return Timing.WaitForSeconds(.1f);
			wasDucking = false;
			duckToStand = true;
			standToDuck = false;
			dtsStarted = false;
			Timing.KillCoroutines("duckToStandP2");
		}
	}

	void CancelDTS()
	{
		wasDucking = false;
		duckToStand = true;
		standToDuck = false;
		dtsStarted = false;
		if (currentSide == PossibleSides.P1)
		{
			Timing.KillCoroutines("duckToStandP1");
		}
		else
		{
			Timing.KillCoroutines("duckToStandP2");
		}

	}

	void CancelTransitionAnims()
	{
		if (currentSide == PossibleSides.P1)
		{
			Timing.KillCoroutines("standToDuckP1");
			Timing.KillCoroutines("duckToStandP1");
		}
		else
		{
			Timing.KillCoroutines("standToDuckP2");
			Timing.KillCoroutines("duckToStandP2");
		}
	}

	void ResetTransitionAnimations()
	{
		standToDuck = false;
		duckToStand = false;
		wasDucking = false;
		dtsStarted = false;
	}


	public void ResetDrainStatus(bool clearGauge)
	{
		for (int i = 0; i < currentGaugesPoints.Length; i++) ResetDrainStatus(clearGauge, i);
	}

	public void ResetDrainStatus(bool clearGauge, int targetGauge)
	{
		storedMove = null;
		storedMoveTime = 0;
		myMoveSetScript.ChangeMoveStances(DCStance);
		if (DCMove != null) CastMove(DCMove, true);

		inhibitGainWhileDraining = false;
		if (gaugeDPS > 0 && (currentGaugesPoints[targetGauge] < 0 || clearGauge)) currentGaugesPoints[targetGauge] = 0;
		gaugeDPS = 0;
		gaugeDrainId = GaugeId.Gauge1;
		currentDrained = 0;
		totalDrain = 0;
		DCMove = null;
	}

	public void ApplyStun(
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs
	)
	{

		if (airRecoveryType == AirRecoveryType.DontRecover
			&& !myPhysicsScript.IsGrounded()
			&& currentSubState == SubStates.Stunned
			&& currentState != PossibleStates.Down)
		{
			stunTime = 1;
		}
		else
		{
			stunTime -= UFE.fixedDeltaTime;
		}

		string standUpAnimation = null;
		Fix64 standUpTime = UFE.config.knockDownOptions.air._standUpTime;
		SubKnockdownOptions knockdownOption = null;

		//soonk addition
		//doubled up for 2p side
		if (currentSide == PossibleSides.P1)
		{
			if (!isDead && currentMove == null && myPhysicsScript.IsGrounded())
			{
				// Knocked Down
				if (currentState == PossibleStates.Down)
				{
					if (myMoveSetScript.basicMoves.standUpFromAirHit.animMap[0].clip != null &&
						(currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitAir, 1)
						|| currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromAirHit, 1)
						|| currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromAirHit, 2)
						|| standUpOverride == StandUpOptions.AirJuggleClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromAirHit, 1);
							standUpTime = UFE.config.knockDownOptions.air._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.air;
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromKnockBack.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitKnockBack, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitKnockBack, 2)
					  || standUpOverride == StandUpOptions.KnockBackClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromKnockBack, 1);
							standUpTime = UFE.config.knockDownOptions.air._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.air;
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromStandingHighHit.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitHighKnockdown, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitHighKnockdown, 2)
					  || standUpOverride == StandUpOptions.HighKnockdownClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.high._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUpFromStandingHighHit);
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingHighHit, 1);
							standUpTime = UFE.config.knockDownOptions.high._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.high;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingHighHit, 1);
                            standUpTime = UFE.config.knockDownOptions.high._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.high;
                            */
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromStandingMidHit.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitMidKnockdown, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitMidKnockdown, 2)
					  || standUpOverride == StandUpOptions.LowKnockdownClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.highLow._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUpFromStandingMidHit);
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingMidHit, 1);
							standUpTime = UFE.config.knockDownOptions.highLow._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.highLow;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingMidHit, 1);
                            standUpTime = UFE.config.knockDownOptions.highLow._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.highLow;
                            */
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromSweep.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitSweep, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitSweep, 2)
					  || standUpOverride == StandUpOptions.SweepClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.sweep._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUpFromSweep);
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromSweep, 1);
							standUpTime = UFE.config.knockDownOptions.sweep._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.sweep;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromSweep, 1);
                            standUpTime = UFE.config.knockDownOptions.sweep._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.sweep;
                            */
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromAirWallBounce.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.airWallBounce, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.airWallBounce, 2)
					  || standUpOverride == StandUpOptions.AirWallBounceClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.wallbounce._standUpTime)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromAirWallBounce, 1);
							standUpTime = UFE.config.knockDownOptions.wallbounce._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.wallbounce;
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromGroundBounce.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromGroundBounce, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.groundBounce, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.groundBounce, 2)
					  || standUpOverride == StandUpOptions.GroundBounceClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromGroundBounce, 1);
							standUpTime = UFE.config.knockDownOptions.air._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.air;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromGroundBounce, 1);
                            standUpTime = UFE.config.knockDownOptions.air._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.air;
                            */
						}
					}
					else
					{
						if (myMoveSetScript.basicMoves.standUp.animMap[0].clip == null)
							Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand Up");

						if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 1);
							standUpTime = UFE.config.knockDownOptions.air._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.air;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 1);
                            standUpTime = UFE.config.knockDownOptions.air._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.air;
                            */
						}
					}
				}
				else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitCrumple, 1)
				  || standUpOverride == StandUpOptions.CrumpleClip)
				{
					if (stunTime <= UFE.config.knockDownOptions.crumple._standUpTime)
					{
						if (myMoveSetScript.basicMoves.standUpFromCrumple.animMap[0].clip != null)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromCrumple, 1);
						}
						else
						{
							if (myMoveSetScript.basicMoves.standUp.animMap[0].clip == null)
								Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand Up");

							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 1);
						}
						standUpTime = UFE.config.knockDownOptions.crumple._standUpTime;
						knockdownOption = UFE.config.knockDownOptions.crumple;
					}
				}
				else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standingWallBounceKnockdown, 1)
				  || standUpOverride == StandUpOptions.StandingWallBounceClip)
				{
					if (stunTime <= UFE.config.knockDownOptions.wallbounce._standUpTime)
					{
						if (myMoveSetScript.basicMoves.standUpFromStandingWallBounce.animMap[0].clip != null)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingWallBounce, 1);
						}
						else
						{
							if (myMoveSetScript.basicMoves.standUp.animMap[0].clip == null)
								Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand Up");

							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 1);
						}
						standUpTime = UFE.config.knockDownOptions.wallbounce._standUpTime;
						knockdownOption = UFE.config.knockDownOptions.wallbounce;
					}
				}

			}
		}
		else
		{
			if (!isDead && currentMove == null && myPhysicsScript.IsGrounded())
			{
				// Knocked Down
				if (currentState == PossibleStates.Down)
				{
					if (myMoveSetScript.basicMoves.standUpFromAirHitP2.animMap[0].clip != null &&
						(currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitAirP2, 1)
						|| currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromAirHitP2, 1)
						|| currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromAirHitP2, 2)
						|| standUpOverride == StandUpOptions.AirJuggleClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromAirHitP2, 1);
							standUpTime = UFE.config.knockDownOptions.air._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.air;
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromKnockBackP2.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitKnockBackP2, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitKnockBackP2, 2)
					  || standUpOverride == StandUpOptions.KnockBackClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromKnockBackP2, 1);
							standUpTime = UFE.config.knockDownOptions.air._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.air;
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromStandingHighHitP2.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitHighKnockdownP2, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitHighKnockdownP2, 2)
					  || standUpOverride == StandUpOptions.HighKnockdownClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.high._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUpFromStandingHighHitP2);
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingHighHitP2, 1);
							standUpTime = UFE.config.knockDownOptions.high._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.high;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingHighHitP2, 1);
                            standUpTime = UFE.config.knockDownOptions.high._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.high;
                            */
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromStandingMidHitP2.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitMidKnockdownP2, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitMidKnockdownP2, 2)
					  || standUpOverride == StandUpOptions.LowKnockdownClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.highLow._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUpFromStandingMidHitP2);
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingMidHitP2, 1);
							standUpTime = UFE.config.knockDownOptions.highLow._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.highLow;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingMidHitP2, 1);
                            standUpTime = UFE.config.knockDownOptions.highLow._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.highLow;
                            */
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromSweepP2.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitSweepP2, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitSweepP2, 2)
					  || standUpOverride == StandUpOptions.SweepClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.sweep._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUpFromSweepP2);
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromSweepP2, 1);
							standUpTime = UFE.config.knockDownOptions.sweep._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.sweep;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromSweepP2, 1);
                            standUpTime = UFE.config.knockDownOptions.sweep._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.sweep;
                            */
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromAirWallBounceP2.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.airWallBounceP2, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.airWallBounceP2, 2)
					  || standUpOverride == StandUpOptions.AirWallBounceClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.wallbounce._standUpTime)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromAirWallBounceP2, 1);
							standUpTime = UFE.config.knockDownOptions.wallbounce._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.wallbounce;
						}
					}
					else if (myMoveSetScript.basicMoves.standUpFromGroundBounceP2.animMap[0].clip != null &&
					  (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromGroundBounceP2, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.groundBounceP2, 1)
					  || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.groundBounceP2, 2)
					  || standUpOverride == StandUpOptions.GroundBounceClip))
					{
						if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromGroundBounceP2, 1);
							standUpTime = UFE.config.knockDownOptions.air._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.air;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromGroundBounceP2, 1);
                            standUpTime = UFE.config.knockDownOptions.air._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.air;
                            */
						}
					}
					else
					{
						if (myMoveSetScript.basicMoves.standUpP2.animMap[0].clip == null)
							Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand UpP2");

						if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
						{
							stunTime = 0;
							currentState = PossibleStates.StandUp;
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpP2, 1);
							standUpTime = UFE.config.knockDownOptions.air._standUpTime;
							knockdownOption = UFE.config.knockDownOptions.air;
							StandUpAnim(standUpAnimation, standUpTime);
							/*
                            standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpP2, 1);
                            standUpTime = UFE.config.knockDownOptions.air._standUpTime;
                            knockdownOption = UFE.config.knockDownOptions.air;
                            */
						}
					}
				}
				else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitCrumpleP2, 1)
				  || standUpOverride == StandUpOptions.CrumpleClip)
				{
					if (stunTime <= UFE.config.knockDownOptions.crumple._standUpTime)
					{
						if (myMoveSetScript.basicMoves.standUpFromCrumpleP2.animMap[0].clip != null)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromCrumpleP2, 1);
						}
						else
						{
							if (myMoveSetScript.basicMoves.standUpP2.animMap[0].clip == null)
								Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand UpP2");

							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpP2, 1);
						}
						standUpTime = UFE.config.knockDownOptions.crumple._standUpTime;
						knockdownOption = UFE.config.knockDownOptions.crumple;
					}
				}
				else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standingWallBounceKnockdownP2, 1)
				  || standUpOverride == StandUpOptions.StandingWallBounceClip)
				{
					if (stunTime <= UFE.config.knockDownOptions.wallbounce._standUpTime)
					{
						if (myMoveSetScript.basicMoves.standUpFromStandingWallBounceP2.animMap[0].clip != null)
						{
							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpFromStandingWallBounceP2, 1);
						}
						else
						{
							if (myMoveSetScript.basicMoves.standUpP2.animMap[0].clip == null)
								Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand UpP2");

							standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUpP2, 1);
						}
						standUpTime = UFE.config.knockDownOptions.wallbounce._standUpTime;
						knockdownOption = UFE.config.knockDownOptions.wallbounce;
					}
				}
				/*
                //soonk - use this to have airborne kills die right when they would stand up
                if (tempDead)
                {
                    tempDead = false;
                    isDead = true;
                    currentLifePoints = 0;
                }
                */
			}
		}

		if (standUpAnimation != null && !myMoveSetScript.IsAnimationPlaying(standUpAnimation))
		{
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUp, standUpAnimation);
			if (myMoveSetScript.basicMoves.standUp.autoSpeed)
			{
				myMoveSetScript.SetAnimationSpeed(standUpAnimation, myMoveSetScript.GetAnimationLength(standUpAnimation) / standUpTime);
			}
			if (knockdownOption != null && knockdownOption.hideHitBoxes) myHitBoxesScript.HideHitBoxes(true);
		}

		if (stunTime <= 0)
		{
			ReleaseStun(previousInputs, currentInputs);
			ncfa_active = false;
		}
	}

	public void CastMove(MoveInfo move, bool overrideCurrentMove = false, bool forceGrounded = false, bool castWarning = false)
	{
		if (move == null) return;
		if (castWarning && !myMoveSetScript.HasMove(move.moveName))
			Debug.LogError("Move '" + move.name + "' could not be found under this character's move set.");
		if (overrideCurrentMove)
		{
			KillCurrentMove();
			MoveInfo newMove = myMoveSetScript.InstantiateMove(move);
			this.SetMove(newMove);
			currentMove.currentFrame = 0;
			currentMove.currentTick = 0;
		}
		else
		{
			storedMove = myMoveSetScript.InstantiateMove(move);
		}
		if (forceGrounded) myPhysicsScript.ForceGrounded();
	}

	public void SetMove(MoveInfo move)
	{
		if (blockStunned) return;
		currentMove = move;

		foreach (HitBox hitBox in myHitBoxesScript.hitBoxes)
		{
			if (hitBox != null && hitBox.bodyPart != BodyPart.none && hitBox.position != null)
			{
				bool visible = hitBox.defaultVisibility;

				if (move != null && move.bodyPartVisibilityChanges != null)
				{
					foreach (BodyPartVisibilityChange visibilityChange in move.bodyPartVisibilityChanges)
					{
						if (visibilityChange.castingFrame == 0 && visibilityChange.bodyPart == hitBox.bodyPart)
						{
							visible = visibilityChange.visible;
							visibilityChange.casted = true;
						}
					}
				}

				hitBox.position.gameObject.SetActive(visible);
			}
		}

		//soonk addition
		//soonk - stand to duck stuff
		CancelTransitionAnims();
		if (currentState == PossibleStates.Crouch)
		{
			standToDuck = true;
			duckToStand = false;
			wasDucking = true;
		}
		if (currentState == PossibleStates.Stand)
		{
			wasDucking = false;
			duckToStand = true;
			standToDuck = false;
			dtsStarted = false;
		}

		UFE.FireMove(currentMove, this);
	}



	//soonk addition
	//stand up stuff
	public void StandUpAnim(string standUpAnimation, Fix64 standUpTime)
	{
		if (myInfo.characterName == "Titan")
		{
			standUpTime *= UFE.config.titanStandupMultiplier;
		}
		standUpTimeTemp = (float)standUpTime;
		myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUp, standUpAnimation);
		if (myMoveSetScript.basicMoves.standUp.autoSpeed)
		{
			myMoveSetScript.SetAnimationSpeed(standUpAnimation, myMoveSetScript.GetAnimationLength(standUpAnimation) / standUpTime);
		}
	}

	void CreatePlayer(VideoClip clip)
	{
		Debug.Log("Create Video Player " + UFE.overlayCam + " " + currentMove.name + " Frame Count " + Time.frameCount);

		if (UFE.overlayCam == null)
			UFE.overlayCam = new GameObject("Finisher Player");

		finisherAudio = UFE.overlayCam.GetComponent<AudioSource>();
		if (finisherAudio == null)
			finisherAudio = UFE.overlayCam.AddComponent<AudioSource>();

		//VideoPlayer oldFinisherPlayer = UFE.overlayCam.GetComponent<VideoPlayer>();
		//if (oldFinisherPlayer)
		//{
		//	Destroy(oldFinisherPlayer);
		//}
		//VideoPlayer vp = UFE.overlayCam.GetComponent<VideoPlayer>();
		//if (vp == null)
		//We need to add a new video player, because it takes 2 frames for the video player to start playing and until then the previus video player either is still playing or displays last frame 
		VideoPlayer vp = UFE.overlayCam.AddComponent<VideoPlayer>();
		vp.enabled = false;

		if (!firstFinisherPlayed)
			vp.playOnAwake = false;


		vp.clip = clip;
		vp.SetTargetAudioSource(0, finisherAudio);
		vp.renderMode = VideoRenderMode.CameraNearPlane;
		vp.targetCamera = Camera.main;
		vp.aspectRatio = VideoAspectRatio.Stretch;
		vp.enabled = true;
		finisherPlayer = vp;

		if (firstFinisherPlayed)
		{
			vp.waitForFirstFrame = false;
			vp.Play();
		}

		//UFE.overlayCam.GetComponent<Camera>().enabled = false;
	}
	//soonk - addition - sakata anti wakeup
	public void ReadMoveSakataStandUp(MoveInfo move)
	{
		if (move == null) return;

		// Check Projectiles
		foreach (Projectile projectile in move.projectiles)
		{
			if (
				!projCasted &&
				projectile.projectilePrefab != null
			)
			{
				projCasted = true;
				projectile.casted = true;
				projectile.gaugeGainOnHit = move.gauges.Length > 0 ? move.gauges[0]._gaugeGainOnHit : 0;
				projectile.gaugeGainOnBlock = move.gauges.Length > 0 ? move.gauges[0]._gaugeGainOnBlock : 0;
				projectile.opGaugeGainOnHit = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnHit : 0;
				projectile.opGaugeGainOnBlock = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnBlock : 0;
				projectile.opGaugeGainOnParry = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnParry : 0;

				FPVector newPos = myHitBoxesScript.GetPosition(projectile.bodyPart);
				if (projectile.fixedZAxis) newPos.z = 0;
				long durationFrames = (int)FPMath.Round(projectile._duration * UFE.config.fps);
				GameObject pTemp = UFE.SpawnGameObject(projectile.projectilePrefab, newPos.ToVector(), Quaternion.identity, durationFrames, true);
				Vector3 newRotation = projectile.projectilePrefab.transform.rotation.eulerAngles;
				newRotation.z = projectile.directionAngle;
				pTemp.transform.rotation = Quaternion.Euler(newRotation);

				ProjectileMoveScript projectileMoveScript = pTemp.AddComponent<ProjectileMoveScript>();
				projectileMoveScript.data = projectile;
				projectileMoveScript.myControlsScript = this;
				projectileMoveScript.mirror = mirror;
				projectileMoveScript.data.moveName = move.moveName;

				projectileMoveScript.fpTransform = pTemp.AddComponent<FPTransform>();
				projectileMoveScript.fpTransform.position = newPos;

				projectileMoveScript.transform.parent = UFE.gameEngine.transform;
				projectiles.Add(projectileMoveScript);
			}
			currentMove = null;
		}
	}

	public void ReadMove(MoveInfo move)
	{
		if (move == null) return;
		potentialParry = 0;
		potentialBlock = false;
		CheckBlocking(false);

		if (move.currentTick == 0)
		{
			if (!myMoveSetScript.AnimationExists(move.name) && !currentMove.finisher)
				Debug.LogError("Animation for move '" + move.name + "' not found!");
			/*if (!myMoveSetScript.AnimationExists(move.name)) {
                Debug.LogWarning("Animation for move '" + move.name + "' not found!");
            }*/

			if (move.disableHeadLook) ToggleHeadLook(false);

			if (myPhysicsScript.IsGrounded())
			{
				myPhysicsScript.isTakingOff = false;
				myPhysicsScript.isLanding = false;
			}

			if (currentState == PossibleStates.NeutralJump ||
				currentState == PossibleStates.ForwardJump ||
				currentState == PossibleStates.BackJump)
			{
				myMoveSetScript.totalAirMoves++;
			}

			Fix64 normalizedTimeConv = myMoveSetScript.GetAnimationNormalizedTime(move.overrideStartupFrame, move);



			//soonk addition
			/*
            if (move.overrideBlendingIn) {
				myMoveSetScript.PlayAnimation(move.name, move._blendingIn, normalizedTimeConv);
			}else{
				myMoveSetScript.PlayAnimation(move.name, myInfo._blendingTime, normalizedTimeConv);
			}
            */
			if (!currentMove.finisher)
			{
				myMoveSetScript.PlayAnimation(move.name, myInfo._blendingTime, normalizedTimeConv);

				myHitBoxesScript.bakeSpeed = move.fixedSpeed ? move.animMap.bakeSpeed : false;
				myHitBoxesScript.animationMaps = move.animMap.animationMaps;
				myHitBoxesScript.UpdateMap(0);
			}
			else if (opControlsScript.isDead)
			{
				CreatePlayer(currentMove.finisherClip);
				if (!firstFinisherPlayed)
				{
					Debug.Log("firstFinisherPlayed = true " + Time.frameCount);
					//firstFinisherPlayed = true;
				}
				else
				{
					//print("FINISHER2");
					Debug.Log("FINISHER2 " + Time.frameCount);
					if (finisherPlayer)
						finisherPlayer.Play();
				}
			}


			applyRootMotion = move.applyRootMotion;
			lockXMotion = move.lockXMotion;
			lockYMotion = move.lockYMotion;
			lockZMotion = move.lockZMotion;

			move.currentTick = move.overrideStartupFrame;
			move.currentFrame = move.overrideStartupFrame;
			move.animationSpeedTemp = move._animationSpeed;

			myMoveSetScript.SetAnimationSpeed(move.name, move._animationSpeed);
			//soonk addition (removal)
			//if (move.overrideBlendingOut) myMoveSetScript.overrideNextBlendingValue = move._blendingOut;

			foreach (GaugeInfo gaugeInfo in move.gauges) RemoveGauge(gaugeInfo._gaugeUsage, (int)gaugeInfo.targetGauge);

			//soonk addition
			//rage stuff
			if (move.startDrainingGauge)
			{
				inRage = true;
				inRageTimes++;
				gaugeDPS = move._gaugeDPS;
				totalDrain = move._totalDrain;
				//DCMove = move.DCMove;
				//DCStance = move.DCStance;
				inhibitGainWhileDraining = move.inhibitGainWhileDraining;
				if (myInfo.playerNum == 1)
				{
					defaultBattleGUI.p1Rage = true;
					//rage achievement
					UFE.p1InGameSaveInfo.achievementsEarned[92] = true;
					if (inRageTimes > 1) UFE.p1InGameSaveInfo.achievementsEarned[86] = true;
				}
				else
				{
					defaultBattleGUI.p2Rage = true;
					//rage achievement
					UFE.p2InGameSaveInfo.achievementsEarned[92] = true;
					if (inRageTimes > 1) UFE.p2InGameSaveInfo.achievementsEarned[86] = true;
				}

			}
			if (move.stopDrainingGauge)
			{
				inRage = false;

				gaugeDPS = 0;
				inhibitGainWhileDraining = false;
				if (myInfo.playerNum == 1)
				{
					defaultBattleGUI.p1Rage = false;
				}
				else
				{
					defaultBattleGUI.p2Rage = false;
				}
			}
		}

		// Next Tick
		if (myMoveSetScript.animationPaused)
		{
			move.currentTick += UFE.fixedDeltaTime * UFE.config.fps * myMoveSetScript.GetAnimationSpeed();
		}
		else
		{
			move.currentTick += UFE.fixedDeltaTime * UFE.config.fps;
		}

		// Test character rotation within frame rotation window
		if (currentMove.autoCorrectRotation && move.currentFrame <= move.frameWindowRotation)
		{
			testCharacterRotation();
		}

		// Assign Current Frame Data Description
		if (move.currentFrame <= move.startUpFrames)
		{
			move.currentFrameData = CurrentFrameData.StartupFrames;
		}
		else if (move.currentFrame > move.startUpFrames && move.currentFrame <= move.startUpFrames + move.activeFrames)
		{
			move.currentFrameData = CurrentFrameData.ActiveFrames;
		}
		else
		{
			move.currentFrameData = CurrentFrameData.RecoveryFrames;
		}

		// Check Speed Key Frames
		if (!move.fixedSpeed)
		{
			foreach (AnimSpeedKeyFrame speedKeyFrame in move.animSpeedKeyFrame)
			{
				if (move.currentFrame >= speedKeyFrame.castingFrame
					&& !myPhysicsScript.freeze)
				{
					myMoveSetScript.SetAnimationSpeed(move.name, speedKeyFrame._speed * move._animationSpeed);
				}
			}
		}

		// Check Projectiles
		foreach (Projectile projectile in move.projectiles)
		{
			if (
				!projectile.casted &&
				projectile.projectilePrefab != null &&
				move.currentFrame >= projectile.castingFrame
			)
			{
				projectile.casted = true;
				projectile.gaugeGainOnHit = move.gauges.Length > 0 ? move.gauges[0]._gaugeGainOnHit : 0;
				projectile.gaugeGainOnBlock = move.gauges.Length > 0 ? move.gauges[0]._gaugeGainOnBlock : 0;
				projectile.opGaugeGainOnHit = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnHit : 0;
				projectile.opGaugeGainOnBlock = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnBlock : 0;
				projectile.opGaugeGainOnParry = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnParry : 0;

				FPVector newPos = myHitBoxesScript.GetPosition(projectile.bodyPart);
				if (projectile.fixedZAxis) newPos.z = 0;
				long durationFrames = (int)FPMath.Round(projectile._duration * UFE.config.fps);
				GameObject pTemp = UFE.SpawnGameObject(projectile.projectilePrefab, newPos.ToVector(), Quaternion.identity, durationFrames, true);
				Vector3 newRotation = projectile.projectilePrefab.transform.rotation.eulerAngles;
				newRotation.z = projectile.directionAngle;
				pTemp.transform.rotation = Quaternion.Euler(newRotation);

				ProjectileMoveScript projectileMoveScript = pTemp.AddComponent<ProjectileMoveScript>();
				projectileMoveScript.data = projectile;
				projectileMoveScript.myControlsScript = this;
				projectileMoveScript.mirror = mirror;
				projectileMoveScript.data.moveName = move.moveName;

				projectileMoveScript.fpTransform = pTemp.AddComponent<FPTransform>();
				projectileMoveScript.fpTransform.position = newPos;

				projectileMoveScript.transform.parent = UFE.gameEngine.transform;
				projectiles.Add(projectileMoveScript);
			}
		}

		// Check Particle Effects
		foreach (MoveParticleEffect particleEffect in move.particleEffects)
		{
			if (
				!particleEffect.casted
				&& particleEffect.particleEffect.prefab != null
				&& move.currentFrame >= particleEffect.castingFrame
			)
			{
				particleEffect.casted = true;
				UFE.FireParticleEffects(currentMove, this, particleEffect);

				long frames = particleEffect.particleEffect.destroyOnMoveOver ? (move.totalFrames - move.currentFrame) : Mathf.RoundToInt(particleEffect.particleEffect.duration * UFE.config.fps);
				Quaternion newRotation = particleEffect.particleEffect.initialRotation != Vector3.zero ? Quaternion.Euler(particleEffect.particleEffect.initialRotation) : Quaternion.identity;
				GameObject pTemp = UFE.SpawnGameObject(particleEffect.particleEffect.prefab, Vector3.zero, newRotation, frames);
				pTemp.transform.rotation = particleEffect.particleEffect.prefab.transform.rotation;

				if (particleEffect.particleEffect.stick)
				{
					Transform targetTransform = myHitBoxesScript.GetTransform(particleEffect.particleEffect.bodyPart);
					pTemp.transform.SetParent(targetTransform);
					pTemp.transform.position = targetTransform.position;
					if (particleEffect.particleEffect.followRotation) pTemp.AddComponent<StickyGameObject>();

				}
				else
				{
					pTemp.transform.SetParent(UFE.gameEngine.transform);
					pTemp.transform.position = myHitBoxesScript.GetPosition(particleEffect.particleEffect.bodyPart).ToVector();
				}

				if (particleEffect.particleEffect.lockLocalPosition) pTemp.transform.localPosition = Vector3.zero;

				Vector3 newPosition = Vector3.zero;
				newPosition.x = particleEffect.particleEffect.positionOffSet.x * -mirror;
				newPosition.y = particleEffect.particleEffect.positionOffSet.y;
				newPosition.z = particleEffect.particleEffect.positionOffSet.z;
				pTemp.transform.localPosition += newPosition;
			}
		}

		// Check Gauge Updates
		foreach (GaugeInfo gaugeInfo in move.gauges)
		{
			if (!gaugeInfo.casted && move.currentFrame >= gaugeInfo.castingFrame)
			{
				AddGauge(gaugeInfo._gaugeGainOnMiss, (int)gaugeInfo.targetGauge);

				if (gaugeInfo.startDrainingGauge)
				{
					gaugeDPS = gaugeInfo._gaugeDPS;
					gaugeDrainId = gaugeInfo.targetGauge;
					totalDrain = gaugeInfo._totalDrain;
					DCMove = gaugeInfo.DCMove;
					DCStance = gaugeInfo.DCStance;
					inhibitGainWhileDraining = gaugeInfo.inhibitGainWhileDraining;
				}
				if (gaugeInfo.stopDrainingGauge)
				{
					gaugeDPS = 0;
					inhibitGainWhileDraining = false;
				}
				moveOnConnect = gaugeInfo.moveOnConnect;
				gaugeInfo.casted = true;
			}
		}

		// Check Applied Forces
		foreach (AppliedForce addedForce in move.appliedForces)
		{
			if (!addedForce.casted && move.currentFrame >= addedForce.castingFrame)
			{
				myPhysicsScript.ResetForces(addedForce.resetPreviousHorizontal, addedForce.resetPreviousVertical);
				myPhysicsScript.AddForce(new FPVector(addedForce._force.x, addedForce._force.y, 0), -mirror);
				addedForce.casted = true;
			}
		}

		// Check Body Part Visibility Changes
		foreach (BodyPartVisibilityChange visibilityChange in move.bodyPartVisibilityChanges)
		{
			if (!visibilityChange.casted && move.currentFrame >= visibilityChange.castingFrame)
			{
				foreach (HitBox hitBox in myHitBoxesScript.hitBoxes)
				{
					if (visibilityChange.bodyPart == hitBox.bodyPart &&
						((mirror == -1 && visibilityChange.left) || (mirror == 1 && visibilityChange.right)))
					{

						UFE.FireBodyVisibilityChange(currentMove, this, visibilityChange, hitBox);
						hitBox.position.gameObject.SetActive(visibilityChange.visible);
						visibilityChange.casted = true;
					}
				}
			}
		}

		// Check SlowMo Effects
		foreach (SlowMoEffect slowMoEffect in move.slowMoEffects)
		{
			if (!slowMoEffect.casted && move.currentFrame >= slowMoEffect.castingFrame)
			{
				UFE.timeScale = (slowMoEffect._percentage / 100) * UFE.config._gameSpeed;
				UFE.DelaySynchronizedAction(UFE.fluxCapacitor.ReturnTimeScale, slowMoEffect._duration);
				slowMoEffect.casted = true;
			}
		}

		// Check Sound Effects
		foreach (SoundEffect soundEffect in move.soundEffects)
		{
			if (!soundEffect.casted && move.currentFrame >= soundEffect.castingFrame)
			{
				UFE.PlaySound(soundEffect.sounds);
				soundEffect.casted = true;
			}
		}

		// Check In Game Alert
		foreach (InGameAlert inGameAlert in move.inGameAlert)
		{
			if (!inGameAlert.casted && move.currentFrame >= inGameAlert.castingFrame)
			{
				UFE.FireAlert(inGameAlert.alert, this);
				inGameAlert.casted = true;
			}
		}

		// Change Stances
		foreach (StanceChange stanceChange in move.stanceChanges)
		{
			if (!stanceChange.casted && move.currentFrame >= stanceChange.castingFrame)
			{
				myMoveSetScript.ChangeMoveStances(stanceChange.newStance);
				stanceChange.casted = true;
			}
		}

		// Check Opponent Override
		foreach (OpponentOverride opponentOverride in move.opponentOverride)
		{
			if (!opponentOverride.casted && move.currentFrame >= opponentOverride.castingFrame)
			{
				if (opponentOverride.stun)
				{
					opControlsScript.stunTime = opponentOverride._stunTime / (Fix64)UFE.config.fps;
					if (opControlsScript.stunTime > 0) opControlsScript.currentSubState = SubStates.Stunned;
				}

				opControlsScript.KillCurrentMove();
				foreach (CharacterSpecificMoves csMove in opponentOverride.characterSpecificMoves)
				{
					if (opInfo.characterName == csMove.characterName)
					{
						opControlsScript.CastMove(csMove.move, true);
						if (opponentOverride.stun) opControlsScript.currentMove.standUpOptions = opponentOverride.standUpOptions;
						opControlsScript.currentMove.hitAnimationOverride = opponentOverride.overrideHitAnimations;
					}
				}
				if (opControlsScript.currentMove == null && opponentOverride.move != null)
				{
					opControlsScript.CastMove(opponentOverride.move, true);
					if (opponentOverride.stun) opControlsScript.currentMove.standUpOptions = opponentOverride.standUpOptions;
					opControlsScript.currentMove.hitAnimationOverride = opponentOverride.overrideHitAnimations;
				}

				opControlsScript.activePullIn = new PullIn();
				FPVector newPos = opponentOverride._position;
				newPos.x *= -mirror;
				opControlsScript.activePullIn.position = worldTransform.position + newPos;
				opControlsScript.activePullIn.speed = opponentOverride.blendSpeed;

				if (opponentOverride.resetAppliedForces)
				{
					opControlsScript.Physics.ResetForces(true, true);
					myPhysicsScript.ResetForces(true, true);
				}

				opponentOverride.casted = true;
			}
		}

#if !UFE_LITE && !UFE_BASIC
		// Character Assist
		foreach (CharacterAssist charAssist in move.characterAssist)
		{
			if (!charAssist.casted && move.currentFrame >= charAssist.castingFrame)
			{

				FPVector offSet = charAssist._spawnPosition;
				offSet.x *= -mirror;
				UFE.SpawnCharacter(charAssist.characterInfo, playerNum, -mirror, (worldTransform.position + offSet), true, charAssist.enterMove, charAssist.exitMove);
				charAssist.casted = true;
			}
		}
#endif

		// Check Camera Movements (cinematics)
		foreach (CameraMovement cameraMovement in move.cameraMovements)
		{
			if (cameraMovement.over) continue;
			if (cameraMovement.casted && !cameraMovement.over && cameraMovement.time >= cameraMovement._duration && UFE.freeCamera)
			{
				cameraMovement.over = true;
				ReleaseCam();
			}
			if (move.currentFrame >= cameraMovement.castingFrame)
			{
				cameraMovement.time += UFE.fixedDeltaTime;
				if (cameraMovement.casted) continue;
				cameraMovement.casted = true;

				UFE.freezePhysics = cameraMovement.freezePhysics;
				cameraScript.cinematicFreeze = cameraMovement.freezePhysics;

				PausePlayAnimation(true, cameraMovement._myAnimationSpeed * .01);
				myPhysicsScript.freeze = cameraMovement.freezePhysics;

				foreach (ControlsScript assist in owner.assists)
				{
					assist.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
					assist.Physics.freeze = cameraMovement.freezePhysics;
				}

				foreach (ControlsScript assist in opControlsScript.assists)
				{
					assist.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
					assist.Physics.freeze = cameraMovement.freezePhysics;
				}

				opControlsScript.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
				opControlsScript.Physics.freeze = cameraMovement.freezePhysics;

				if (isAssist)
				{
					owner.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
					owner.Physics.freeze = cameraMovement.freezePhysics;
				}


				if (cameraMovement.cinematicType == CinematicType.CameraEditor)
				{
					cameraMovement.position.x *= -mirror;
					Vector3 targetPosition = transform.TransformPoint(cameraMovement.position);
					Vector3 targetRotation = cameraMovement.rotation;
					targetRotation.y *= -mirror;
					targetRotation.z *= -mirror;
					cameraScript.MoveCameraToLocation(targetPosition,
													  targetRotation,
													  cameraMovement.fieldOfView,
													  cameraMovement.camSpeed, gameObject.name);

				}
				else if (cameraMovement.cinematicType == CinematicType.Prefab)
				{
					cameraScript.SetCameraOwner(gameObject.name);
					emulatedCam = UFE.SpawnGameObject(cameraMovement.prefab, transform.position, Quaternion.identity);

				}
				else if (cameraMovement.cinematicType == CinematicType.AnimationFile)
				{
					emulatedCam = new GameObject();
					emulatedCam.name = "Camera Parent";
					emulatedCam.transform.parent = transform;
					emulatedCam.transform.localPosition = cameraMovement.gameObjectPosition;
					emulatedCam.AddComponent(typeof(Animation));
					emulatedCam.GetComponent<Animation>().AddClip(cameraMovement.animationClip, "cam");
					emulatedCam.GetComponent<Animation>()["cam"].speed = cameraMovement.camAnimationSpeed;
					emulatedCam.GetComponent<Animation>().Play("cam");

					Camera.main.transform.parent = emulatedCam.transform;
					cameraScript.MoveCameraToLocation(cameraMovement.position,
													  cameraMovement.rotation,
													  cameraMovement.fieldOfView,
													  cameraMovement.blendSpeed, gameObject.name);

				}
			}
		}

		// Check Invincible Body Parts
		if (move.invincibleBodyParts.Length > 0)
		{
			foreach (InvincibleBodyParts invBodyPart in move.invincibleBodyParts)
			{
				if (move.currentFrame >= invBodyPart.activeFramesBegin &&
					move.currentFrame < invBodyPart.activeFramesEnds)
				{
					if (invBodyPart.completelyInvincible)
					{
						myHitBoxesScript.HideHitBoxes(true);
					}
					else
					{
						myHitBoxesScript.HideHitBoxes(invBodyPart.hitBoxes, true);
					}
					ignoreCollisionMass = invBodyPart.ignoreBodyColliders;
				}
				if (move.currentFrame >= invBodyPart.activeFramesEnds)
				{
					if (invBodyPart.completelyInvincible)
					{
						myHitBoxesScript.HideHitBoxes(false);
					}
					else
					{
						myHitBoxesScript.HideHitBoxes(invBodyPart.hitBoxes, false);
					}
					ignoreCollisionMass = false;
				}
			}
		}

		// Check Blockable Area
		if (move.blockableArea.bodyPart != BodyPart.none)
		{
			if (move.currentFrame >= move.blockableArea.activeFramesBegin &&
				move.currentFrame < move.blockableArea.activeFramesEnds)
			{
				myHitBoxesScript.blockableArea = move.blockableArea;
				myHitBoxesScript.blockableArea.position = myHitBoxesScript.GetPosition(myHitBoxesScript.blockableArea.bodyPart);

				if (!opControlsScript.isBlocking
					&& !opControlsScript.blockStunned
					&& opControlsScript.currentSubState != SubStates.Stunned
					&& opControlsScript.HitBoxes.TestCollision(myHitBoxesScript.blockableArea, mirror).Length > 0)
				{
					opControlsScript.CheckBlocking(true);
				}
			}
			else if (move.currentFrame >= move.blockableArea.activeFramesEnds)
			{
				if (UFE.config.blockOptions.blockType == BlockType.HoldBack ||
					UFE.config.blockOptions.blockType == BlockType.AutoBlock) opControlsScript.CheckBlocking(false);
			}
		}

		// Check Frame Links
		foreach (FrameLink frameLink in move.frameLinks)
		{
			if (move.currentFrame >= frameLink.activeFramesBegins &&
				move.currentFrame <= frameLink.activeFramesEnds)
			{
				if (frameLink.linkType == LinkType.NoConditions ||
					(frameLink.linkType == LinkType.HitConfirm &&
					((currentMove.hitConfirmOnStrike && frameLink.onStrike) ||
					(currentMove.hitConfirmOnBlock && frameLink.onBlock) ||
					(currentMove.hitConfirmOnParry && frameLink.onParry))))
				{
					frameLink.cancelable = true;
				}
			}
			else
			{
				frameLink.cancelable = false;
			}
		}

		// Check Hits
		if (!UFE.freezePhysics)
		{
			foreach (ControlsScript assist in opControlsScript.assists) CheckHits(move, assist);
			CheckHits(move, opControlsScript);
		}

		// Next Frame
		move.currentFrame = (int)FPMath.Floor(move.currentTick);
		if (move.finisher && !finisherReady)
		{
			move.currentTick = 1;
		}

		// Kill Move
		if (move.currentFrame >= move.totalFrames)
		{
			//soonk
			//if (move.name == "Intro") {
			if (move.name == "Intro" || move.name == "IntroP2")
			{
				introPlayed = true;
				if (opControlsScript.introPlayed) UFE.CastNewRound(2);
			}
			if (move.armorOptions.hitsTaken > 0) comboHits = 0;
			KillCurrentMove();

			// Assist - Despawn after Exit Move
			if (exitMove != null && move.name == exitMove.name)
			{
				exitMove = null;
				SetActive(false);

				worldTransform.position = new FPVector(-999, -999, 0);
				transform.position = worldTransform.position.ToVector();
			}
		}
	}

	public void CheckHits(MoveInfo move, ControlsScript opControlsScript)
	{
		HurtBox[] activeHurtBoxes = null;
		foreach (Hit hit in move.hits)
		{
			//soonk - test adding -1 to fix the visual editor
			if (move.currentFrame >= hit.activeFramesBegin - 1 &&
				move.currentFrame <= hit.activeFramesEnds - 1)
			{
				if (hit.hurtBoxes.Length > 0)
				{
					activeHurtBoxes = hit.hurtBoxes;
					if (hit.impactList == null) hit.impactList = new List<ControlsScript>();

					if ((!hit.continuousHit && hit.impactList.Contains(opControlsScript)) || (hit.continuousHit && move.currentTick < move.currentFrame)) continue;
					if (!opControlsScript.ValidateHit(hit)) continue;

					foreach (HurtBox hurtBox in activeHurtBoxes)
					{
						hurtBox.position = myHitBoxesScript.GetPosition(hurtBox.bodyPart);
						hurtBox.rendererBounds = myHitBoxesScript.GetBounds();
					}

					FPVector[] collisionVectors = opControlsScript.HitBoxes != null ? opControlsScript.HitBoxes.TestCollision(activeHurtBoxes, hit.hitConfirmType, mirror) : new FPVector[0];
					if (collisionVectors.Length > 0)
					{ // HURTBOX TEST
						Fix64 newAnimSpeed = GetHitAnimationSpeed(hit.hitStrength);
						Fix64 freezingTime = GetHitFreezingTime(hit.hitStrength);

						// Tech Throw
						if (hit.hitConfirmType == HitConfirmType.Throw
							&& hit.techable
							&& opControlsScript.currentMove != null
							&& opControlsScript.currentMove.IsThrow(true)
							)
						{
							if (playerNum == 1)
							{
								UFE.p1InGameSaveInfo.gameStats["timesTechedThrow"]++;
							}
							else
							{
								UFE.p2InGameSaveInfo.gameStats["timesTechedThrow"]++;
							}
							CastMove(hit.techMove, true);
							opControlsScript.CastMove(opControlsScript.currentMove.GetTechMove(), true);
							return;

							// Throw
						}
						// soonk - fixed throw during stun
						else if (hit.hitConfirmType == HitConfirmType.Throw)
						{
							if (playerNum == 1)
							{
								UFE.p1InGameSaveInfo.gameStats["timesSideSwitched"]++;
							}
							else
							{
								UFE.p2InGameSaveInfo.gameStats["timesSideSwitched"]++;
							}
							if (!opControlsScript.blockStunned)
							{
								CastMove(hit.throwMove, true);
								return;
							}
							else
							{
								return;
							}



							// Parry
						}
						else if (opControlsScript.potentialParry > 0
								 && opControlsScript.currentMove == null
								 && hit.hitConfirmType != HitConfirmType.Throw
								 && opControlsScript.TestParryStances(hit.hitType)
								 )
						{
							opControlsScript.GetHitParry(hit, move.totalFrames - move.currentFrame, collisionVectors, this);
							foreach (GaugeInfo gaugeInfo in move.gauges)
							{
								opControlsScript.AddGauge(gaugeInfo._opGaugeGainOnParry, (int)gaugeInfo.targetGauge);
							}
							move.hitConfirmOnParry = true;

							// Block
							//soonk addition
						}
						else if ((opControlsScript.currentSubState != SubStates.Stunned || hit.ncfa)
								 //} else if (opControlsScript.currentSubState != SubStates.Stunned
								 && opControlsScript.currentMove == null
								 && opControlsScript.isBlocking
								 && opControlsScript.TestBlockStances(hit.hitType)
								 && !hit.unblockable
								 )
						{
							opControlsScript.GetHitBlocking(hit, move.totalFrames - move.currentFrame, collisionVectors, false, this);
							foreach (GaugeInfo gaugeInfo in move.gauges)
							{
								AddGauge(gaugeInfo._gaugeGainOnBlock, (int)gaugeInfo.targetGauge);
								AddGaugeConnect(-gaugeInfo._gaugeUsageOnConnect, (int)gaugeInfo.targetGauge);
								opControlsScript.AddGauge(gaugeInfo._opGaugeGainOnBlock, (int)gaugeInfo.targetGauge);
							}
							move.hitConfirmOnBlock = true;

							if (hit.overrideHitEffectsBlock)
							{
								newAnimSpeed = hit.hitEffectsBlock._animationSpeed;
								//soonk - airborne freeze time change
								if (opControlsScript.myPhysicsScript.IsGrounded())
								{
									freezingTime = hit.hitEffectsBlock._freezingTime;
								}
								else
								{
									freezingTime = hit.hitEffectsBlock._freezingTimeAirborne;
								}
								//freezingTime = hit.hitEffectsBlock._freezingTime;

							}
							//soonk addition
							//trip guard stuff
							move.moveConnected = true;
							tripGuard = false;

							// Hit
						}
						else
						{
							//soonk addition
							//Added to check if player has been hit after 'death' and to force them to stay down if so
							if (opControlsScript.isDead)
							{
								//UFE.config.lockMovements = true;
								if (opControlsScript.hitWhileDead == false)
								{
									if (opControlsScript.myPhysicsScript.IsGrounded())
									{
										Debug.Log($"Hit While dead set for {opControlsScript.gameObject.name}");
										opControlsScript.hitWhileDead = true;
										opControlsScript.stunTime = 999;
										opControlsScript.currentState = PossibleStates.Down;
										//Timing.RunCoroutine(_CheckForFinisher(), Segment.SlowUpdate, "Finisher");
									}
									else
									{
										print("dead in air");
										//opControlsScript.currentState = PossibleStates.StandUp;
									}

									//myHitBoxesScript.HideHitBoxes(true);
								}
								else
								{
									opControlsScript.stunTime = 999;
									opControlsScript.currentState = PossibleStates.Down;
								}
							}
							opControlsScript.CancelTransitionAnims();
							opControlsScript.ResetTransitionAnimations();

							opControlsScript.GetHit(hit, move.totalFrames - move.currentFrame, collisionVectors, false, this);
							foreach (GaugeInfo gaugeInfo in move.gauges)
							{
								AddGauge(gaugeInfo._gaugeGainOnHit, (int)gaugeInfo.targetGauge);
								AddGaugeConnect(-gaugeInfo._gaugeUsageOnConnect, (int)gaugeInfo.targetGauge);
								opControlsScript.AddGauge(gaugeInfo._opGaugeGainOnHit, (int)gaugeInfo.targetGauge);
							}

							//soonk addition
							//add unique moves to list on hit only
							if (!uniqueMoves.Contains(move.name))
							{
								uniqueMoves.Add(move.name);
								moveTypes.Add(move.moveSideType.ToString());
							}

							// crouch attack count
							if (move.crouchMove)
							{
								if (playerNum == 1)
								{
									UFE.p1InGameSaveInfo.gameStats["crouchAttackCount"]++;
								}
								else
								{
									UFE.p2InGameSaveInfo.gameStats["crouchAttackCount"]++;
								}
							}

							// AA attack count
							if (move.antiAir)
							{
								if (playerNum == 1)
								{
									UFE.p1InGameSaveInfo.gameStats["aaAttackCount"]++;
								}
								else
								{
									UFE.p2InGameSaveInfo.gameStats["aaAttackCount"]++;
								}
							}

							if (hit.pullSelfIn.enemyBodyPart != BodyPart.none && hit.pullSelfIn.characterBodyPart != BodyPart.none)
							{
								FPVector newPos = opControlsScript.HitBoxes.GetPosition(hit.pullSelfIn.enemyBodyPart);
								if (newPos != FPVector.zero)
								{
									activePullIn = new PullIn();
									activePullIn.position = worldTransform.position + (newPos - myHitBoxesScript.GetPosition(hit.pullSelfIn.characterBodyPart));
									activePullIn.speed = hit.pullSelfIn.speed;
									activePullIn.forceStand = hit.pullEnemyIn.forceStand;
									activePullIn.position.z = 0;
									if (hit.pullEnemyIn.forceStand)
									{
										activePullIn.position.y = 0;
										myPhysicsScript.ForceGrounded();
									}
								}
							}
							move.hitConfirmOnStrike = true;

							if (hit.overrideHitEffects)
							{
								newAnimSpeed = hit.hitEffects._animationSpeed;
								if (opControlsScript.myPhysicsScript.IsGrounded())
								{
									freezingTime = hit.hitEffects._freezingTime;
								}
								else
								{
									freezingTime = hit.hitEffects._freezingTimeAirborne;
								}
								//freezingTime = hit.hitEffects._freezingTime;
							}

							//soonk addition
							//soonk - trip guard stuff
							move.moveConnected = true;
							tripGuard = false;

							myPhysicsScript.ResetForces(hit.resetPreviousHorizontal, hit.resetPreviousVertical);
							myPhysicsScript.AddForce(hit._appliedForce, -mirror);
						}
						//soonk addition (moved above)
						//myPhysicsScript.ResetForces(hit.resetPreviousHorizontal, hit.resetPreviousVertical);
						//myPhysicsScript.AddForce(hit._appliedForce, -mirror);

						// Test position boundaries
						if ((opControlsScript.worldTransform.position.x >= UFE.config.selectedStage.position.x + UFE.config.selectedStage._rightBoundary - 2 ||
							 opControlsScript.worldTransform.position.x <= UFE.config.selectedStage.position.x + UFE.config.selectedStage._leftBoundary + 2)
							&& myPhysicsScript.IsGrounded()
							&& !UFE.config.comboOptions.neverCornerPush && hit.cornerPush
							)
						{

							myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, false);
							myPhysicsScript.AddForce(
								new FPVector(hit._pushForce.x + ((Fix64)opControlsScript.Physics.airTime * opInfo.physics._friction), 0, 0), mirror);
						}

						// Apply freezing effect
						if (opControlsScript.Physics.freeze)
						{
							HitPause(newAnimSpeed * .01);
							UFE.DelaySynchronizedAction(this.HitUnpause, freezingTime);
						}
						hit.impactList.Add(opControlsScript);
					};
				}
			}
			myHitBoxesScript.activeHurtBoxes = activeHurtBoxes;
		}

	}


	// Imediately cancels any move being executed
	public void KillCurrentMove()
	{
		if (currentMove == null) return;
		currentMove.currentFrame = 0;
		currentMove.currentTick = 0;

		myHitBoxesScript.activeHurtBoxes = null;
		myHitBoxesScript.blockableArea = null;

		ignoreCollisionMass = false;
		if (UFE.config.blockOptions.blockType == BlockType.HoldBack ||
			UFE.config.blockOptions.blockType == BlockType.AutoBlock) opControlsScript.CheckBlocking(false);

		//soonk addition (removal)
		/*
		if (currentMove.disableHeadLook) ToggleHeadLook(true);

        if (mirror == -1) {
            if (currentMove.invertRotationLeft) InvertRotation();
            if (currentMove.forceMirrorLeft) ForceMirror(false);
        }
        else if (mirror == 1) {
            if (currentMove.invertRotationRight) InvertRotation();
            if (currentMove.forceMirrorRight) ForceMirror(true);
        }
        */

		fixCharacterRotation();

		if (stunTime > 0)
		{
			standUpOverride = currentMove.standUpOptions;
			if (standUpOverride != StandUpOptions.None) currentState = PossibleStates.Down;
		}

		this.SetMove(null);
		ReleaseCam();
	}

	// Release character to be playable again
	private void ReleaseStun(
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs
	)
	{
		if (currentSubState != SubStates.Stunned && !blockStunned) return;
		//soonk addition
		/*
		if (!isBlocking && comboHits > 1 && UFE.config.comboOptions.comboDisplayMode == ComboDisplayMode.ShowAfterComboExecution){
			UFE.FireAlert(UFE.config.selectedLanguage.combo, opControlsScript);
		}
        */
		if (comboHits > 1 && UFE.config.comboOptions.comboDisplayMode == ComboDisplayMode.ShowAfterComboExecution)
		{
			//soonk - changed from .combo for combo damage readout
			comboDamage = Mathf.Round((float)comboDamage);
			UFE.FireAlert(UFE.config.selectedLanguage.comboDamage, opControlsScript);
		}

		// soonk - checks and stores highest combo damage, hits, and adds wallbounces
		if (playerNum == 1)
		{
			if (comboDamage > UFE.p2InGameSaveInfo.gameStats["highestComboDmg"]) UFE.p2InGameSaveInfo.gameStats["highestComboDmg"] = (int)comboDamage;
			if (comboHits > UFE.p2InGameSaveInfo.gameStats["mostComboHits"]) UFE.p2InGameSaveInfo.gameStats["mostComboHits"] = comboHits;
			UFE.p2InGameSaveInfo.gameStats["wallBouncesCaused"] += myPhysicsScript.wallBounceTimes;
		}
		else
		{
			if (comboDamage > UFE.p1InGameSaveInfo.gameStats["highestComboDmg"]) UFE.p1InGameSaveInfo.gameStats["highestComboDmg"] = (int)comboDamage;
			if (comboHits > UFE.p1InGameSaveInfo.gameStats["mostComboHits"]) UFE.p1InGameSaveInfo.gameStats["mostComboHits"] = comboHits;
			UFE.p1InGameSaveInfo.gameStats["wallBouncesCaused"] += myPhysicsScript.wallBounceTimes;
		}

		currentHit = null;
		currentSubState = SubStates.Resting;
		blockStunned = false;
		stunTime = 0;
		comboHits = 0;
		comboDamage = 0;
		comboHitDamage = 0;
		airJuggleHits = 0;
		consecutiveCrumple = 0;
		CheckBlocking(false);

		standUpOverride = StandUpOptions.None;

		myPhysicsScript.ResetWeight();
		myPhysicsScript.isWallBouncing = false;
		myPhysicsScript.wallBounceTimes = 0;
		myPhysicsScript.overrideStunAnimation = null;
		myPhysicsScript.overrideAirAnimation = false;

		if (!myPhysicsScript.IsGrounded()) isAirRecovering = true;

		if (!isDead) ToggleHeadLook(true);

		//soonk addition
		if (myPhysicsScript.IsGrounded()) currentState = PossibleStates.StandUp;
		//if (myPhysicsScript.IsGrounded()) currentState = PossibleStates.Stand;

		if (!isAssist) translateInputs(previousInputs, currentInputs);
	}

	private void ReleaseCam()
	{
		if (cameraScript.GetCameraOwner() != gameObject.name) return;
		if (outroPlayed && UFE.config.roundOptions.freezeCamAfterOutro) return;
		Camera.main.transform.parent = null;

		if (emulatedCam != null)
		{
			UFE.DestroyGameObject(emulatedCam);
		}

		UFE.freezePhysics = false;
		cameraScript.ReleaseCam();

		PausePlayAnimation(false);
		myPhysicsScript.freeze = false;

		foreach (ControlsScript assist in owner.assists)
		{
			assist.PausePlayAnimation(false);
			assist.Physics.freeze = false;
		}

		foreach (ControlsScript assist in opControlsScript.assists)
		{
			assist.PausePlayAnimation(false);
			assist.Physics.freeze = false;
		}

		opControlsScript.PausePlayAnimation(false);
		opControlsScript.Physics.freeze = false;

		if (isAssist)
		{
			owner.PausePlayAnimation(false);
			owner.Physics.freeze = false;
		}
	}

	public bool TestBlockStances(HitType hitType)
	{
		if (UFE.config.blockOptions.blockType == BlockType.None) return false;
		if ((hitType == HitType.Mid || hitType == HitType.MidKnockdown || hitType == HitType.Launcher) && myPhysicsScript.IsGrounded()) return true;
		//soonk addition
		/*
		if ((hitType == HitType.Overhead || hitType == HitType.HighKnockdown) && currentState == PossibleStates.Crouch) return false;
		if ((hitType == HitType.Sweep || hitType == HitType.Low) && currentState != PossibleStates.Crouch) return false;
        */
		if (hitType == HitType.Overhead && currentState == PossibleStates.Crouch) return false;
		if ((hitType == HitType.Sweep || hitType == HitType.Low) && (currentState != PossibleStates.Crouch && !downHeld)) return false;
		if (!UFE.config.blockOptions.allowAirBlock && !myPhysicsScript.IsGrounded()) return false;
		return true;
	}

	public bool TestParryStances(HitType hitType)
	{
		if (UFE.config.blockOptions.parryType == ParryType.None) return false;
		if ((hitType == HitType.Mid || hitType == HitType.MidKnockdown || hitType == HitType.Launcher) && myPhysicsScript.IsGrounded()) return true;
		if ((hitType == HitType.Overhead || hitType == HitType.HighKnockdown) && currentState == PossibleStates.Crouch) return false;
		if ((hitType == HitType.Sweep || hitType == HitType.Low) && currentState != PossibleStates.Crouch) return false;
		if (!UFE.config.blockOptions.allowAirParry && !myPhysicsScript.IsGrounded()) return false;
		return true;
	}

	public void CheckBlocking(bool flag)
	{
		if (myPhysicsScript.freeze) return;
		if (myPhysicsScript.isTakingOff) return;
		if (flag)
		{
			if (potentialBlock)
			{
				if (currentMove != null)
				{
					potentialBlock = false;
					return;
				}
				//soonk addition
				if (currentSide == PossibleSides.P1)
				{
					if (currentState == PossibleStates.Crouch && !ncfa_active)
					{
						if (myMoveSetScript.basicMoves.blockingCrouchingPose.animMap[0].clip == null)
							Debug.LogError("Blocking Crouching Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Crouching Pose");
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingCrouchingPose, false);
						isBlocking = true;
						standToDuck = true;
						duckToStand = false;
						wasDucking = true;
					}
					else if (currentState == PossibleStates.Stand && !ncfa_active)
					{
						if (myMoveSetScript.basicMoves.blockingHighPose.animMap[0].clip == null)
							Debug.LogError("Blocking High Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking High Pose");
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingHighPose, false);
						isBlocking = true;
						wasDucking = false;
						duckToStand = true;
						standToDuck = false;
					}
					else if (currentState == PossibleStates.StandUp && !ncfa_active)
					{
						isBlocking = true;
					}
					else if (!myPhysicsScript.IsGrounded() && UFE.config.blockOptions.allowAirBlock)
					{
						if (myMoveSetScript.basicMoves.blockingAirPose.animMap[0].clip == null)
							Debug.LogError("Blocking Air Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Air Pose");
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingAirPose, false);
						isBlocking = true;
					}
				}
				else
				{
					if (currentState == PossibleStates.Crouch && !ncfa_active)
					{
						if (myMoveSetScript.basicMoves.blockingCrouchingPoseP2.animMap[0].clip == null)
							Debug.LogError("Blocking Crouching Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Crouching Pose P2");
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingCrouchingPoseP2, false);
						isBlocking = true;
						standToDuck = true;
						duckToStand = false;
						wasDucking = true;
					}
					else if (currentState == PossibleStates.Stand && !ncfa_active)
					{
						if (myMoveSetScript.basicMoves.blockingHighPoseP2.animMap[0].clip == null)
							Debug.LogError("Blocking High Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking High Pose P2");
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingHighPoseP2, false);
						isBlocking = true;
						wasDucking = false;
						duckToStand = true;
						standToDuck = false;
					}
					else if (currentState == PossibleStates.StandUp && !ncfa_active)
					{
						isBlocking = true;
					}
					else if (!myPhysicsScript.IsGrounded() && UFE.config.blockOptions.allowAirBlock)
					{
						if (myMoveSetScript.basicMoves.blockingAirPoseP2.animMap[0].clip == null)
							Debug.LogError("Blocking Air Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Air Pose P2");
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingAirPoseP2, false);
						isBlocking = true;
					}
				}
				if (ncfa_active) isBlocking = true;
				// achievement tracker
				hasBlocked = true;
			}
		}
		else if (!blockStunned)
		{
			isBlocking = false;
		}
	}

	private void HighlightOn(GameObject target, bool flag)
	{
		Renderer[] charRenders = target.GetComponentsInChildren<Renderer>();
		if (flag && !lit)
		{
			lit = true;
			foreach (Renderer charRender in charRenders)
			{
				charRender.material.shader = Shader.Find("VertexLit");
				charRender.material.color = UFE.config.blockOptions.parryColor;
			}
		}
		else if (lit)
		{
			lit = false;
			for (int i = 0; i < charRenders.Length; i++)
			{
				charRenders[i].material.shader = normalShaders[i];
				charRenders[i].material.color = normalColors[i];
			}
		}
	}

	private void HighlightOff()
	{
		HighlightOn(character, false);
	}

	public bool ValidateHit(Hit hit)
	{
		if (comboHits >= UFE.config.comboOptions.maxCombo) return false;
		if (!hit.groundHit && myPhysicsScript.IsGrounded()) return false;
		if (!hit.crouchingHit && currentState == PossibleStates.Crouch) return false;
		if (!hit.airHit && currentState != PossibleStates.Stand && currentState != PossibleStates.Crouch && !myPhysicsScript.IsGrounded()) return false;
		if (!hit.stunHit && currentSubState == SubStates.Stunned) return false;
		if (!hit.downHit && currentState == PossibleStates.Down) return false;
		if (myMoveSetScript != null && !myMoveSetScript.ValidadeBasicMove(hit.opponentConditions, this)) return false;
		if (myMoveSetScript != null && !myMoveSetScript.ValidateMoveStances(hit.opponentConditions, this)) return false;

		return true;
	}

	public void GetHitParry(Hit hit, int remainingFrames, FPVector[] location, ControlsScript attacker)
	{
		UFE.FireAlert(UFE.config.selectedLanguage.parry, this);

		BasicMoveInfo currentHitInfo = myMoveSetScript.basicMoves.parryHigh;
		blockStunned = true;
		currentSubState = SubStates.Blocking;

		myHitBoxesScript.isHit = true;

		if (!UFE.config.blockOptions.easyParry)
		{
			potentialParry = 0;
		}

		if (UFE.config.blockOptions.resetButtonSequence)
		{
			myMoveSetScript.ClearLastButtonSequence();
		}

		if (UFE.config.blockOptions.parryStunType == ParryStunType.Fixed)
		{
			stunTime = (Fix64)UFE.config.blockOptions.parryStunFrames / (Fix64)UFE.config.fps;
		}
		else
		{
			int stunFrames = 0;
			if (hit.hitStunType == HitStunType.FrameAdvantage)
			{
				stunFrames = hit.frameAdvantageOnBlock + remainingFrames;
				stunFrames *= (UFE.config.blockOptions.parryStunFrames / 100);
				if (stunFrames < 1) stunFrames = 1;
				stunTime = (Fix64)stunFrames / (Fix64)UFE.config.fps;
			}
			else if (hit.hitStunType == HitStunType.Frames)
			{
				stunFrames = (int)hit._hitStunOnBlock;
				stunFrames = (int)FPMath.Round(((Fix64)(stunFrames * UFE.config.blockOptions.parryStunFrames) / (Fix64)100));
				if (stunFrames < 1) stunFrames = 1;
				stunTime = (Fix64)stunFrames / (Fix64)UFE.config.fps;
			}
			else
			{
				stunTime = hit._hitStunOnBlock * ((Fix64)UFE.config.blockOptions.parryStunFrames / (Fix64)100);
			}
		}

		UFE.FireParry(myHitBoxesScript.GetStrokeHitBox(), attacker.currentMove, this);

		// Create hit parry effect
		GameObject particle = UFE.config.blockOptions.parryHitEffects.hitParticle;
		Fix64 killTime = UFE.config.blockOptions.parryHitEffects.killTime;
		AudioClip soundEffect = UFE.config.blockOptions.parryHitEffects.hitSound;
		if (location.Length > 0 && particle != null)
		{
			HitEffectSpawnPoint spawnPoint = UFE.config.blockOptions.parryHitEffects.spawnPoint;
			if (hit.overrideEffectSpawnPoint) spawnPoint = hit.spawnPoint;

			long frames = (long)FPMath.Round(killTime * UFE.config.fps);
			GameObject pTemp = UFE.SpawnGameObject(particle, GetParticleSpawnPoint(spawnPoint, location), Quaternion.identity, frames);
			pTemp.transform.rotation = particle.transform.rotation;

			if (UFE.config.blockOptions.parryHitEffects.mirrorOn2PSide && mirror > 0)
			{
				pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
			}

			//pTemp.transform.localScale = new Vector3(-mirror, 1, 1);
			pTemp.transform.parent = UFE.gameEngine.transform;
		}
		UFE.PlaySound(soundEffect);

		// Shake Options
		shakeCamera = UFE.config.blockOptions.parryHitEffects.shakeCameraOnHit;
		shakeCharacter = UFE.config.blockOptions.parryHitEffects.shakeCharacterOnHit;
		shakeDensity = UFE.config.blockOptions.parryHitEffects._shakeDensity;
		shakeCameraDensity = UFE.config.blockOptions.parryHitEffects._shakeCameraDensity;



		// Get correct animation according to stance
		if (currentState == PossibleStates.Crouch)
		{
			currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryCrouching, hit);
			currentHitInfo = myMoveSetScript.basicMoves.parryCrouching;
			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
				Debug.LogError("Parry Crouching animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Crouching");
		}
		else if (currentState == PossibleStates.Stand)
		{
			HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
			if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.parryLow.animMap[0].clip != null)
			{
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryLow, hit);
				currentHitInfo = myMoveSetScript.basicMoves.parryLow;

			}
			else
			{
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryHigh, hit);
				currentHitInfo = myMoveSetScript.basicMoves.parryHigh;
				if (!myMoveSetScript.AnimationExists(currentHitAnimation))
					Debug.LogError("Parry High animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Standing");

			}
		}
		else if (!myPhysicsScript.IsGrounded())
		{
			currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryAir, hit);
			currentHitInfo = myMoveSetScript.basicMoves.parryAir;
			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
				Debug.LogError("Parry Air animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Air");
		}

		myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation);
		if (currentHitInfo.autoSpeed)
		{
			myMoveSetScript.SetAnimationSpeed(currentHitAnimation, (myMoveSetScript.GetAnimationLength(currentHitAnimation) / stunTime));
		}

		// Highlight effect when parry
		if (UFE.config.blockOptions.highlightWhenParry)
		{
			HighlightOn(gameObject, true);
			UFE.DelaySynchronizedAction(this.HighlightOff, 0.2);
		}

		// Freeze screen depending on how strong the hit was
		HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01);
		UFE.DelaySynchronizedAction(this.HitUnpause, GetHitFreezingTime(hit.hitStrength));

		// Reset hit to allow for another hit while the character is still stunned
		Fix64 spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.Small)
		{
			spaceBetweenHits = 1.1;
		}
		else if (hit.spaceBetweenHits == Sizes.Medium)
		{
			spaceBetweenHits = 1.3;
		}
		else if (hit.spaceBetweenHits == Sizes.High)
		{
			spaceBetweenHits = 1.7;
		}

		if (UFE.config.blockOptions.parryHitEffects.autoHitStop)
		{
			UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, GetHitFreezingTime(hit.hitStrength) * spaceBetweenHits);
		}
		else
		{
			UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, UFE.config.blockOptions.parryHitEffects._hitStop * spaceBetweenHits);
		}

		// Add force to the move
		myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);

		if (!UFE.config.blockOptions.ignoreAppliedForceParry)
			myPhysicsScript.AddForce(new FPVector(hit._pushForce.x, 0, 0), -opControlsScript.mirror);

	}

	public void GetHitBlocking(Hit hit, int remainingFrames, FPVector[] location, bool ignoreDirection, ControlsScript attacker)
	{
		currentHit = hit;
		// Lose life
		if (hit._damageOnBlock >= currentLifePoints)
		{
			GetHit(hit, remainingFrames, location, ignoreDirection, attacker);
			return;
		}
		else
		{
			Fix64 damage = hit._damageOnBlock;
			if (hit.damageType == DamageType.Percentage) damage = myInfo.lifePoints * (damage / 100);
			DamageMe(damage);
		}

		blockStunned = true;
		currentSubState = SubStates.Blocking;
		myHitBoxesScript.isHit = true;

		int stunFrames = 0;
		BasicMoveInfo currentHitInfo = myMoveSetScript.basicMoves.blockingHighHit;

		if (hit.hitStunType == HitStunType.FrameAdvantage)
		{
			//soonk addition
			//fix for multi hit moves
			if (hit.multiHitMove)
			{
				stunFrames = hit.frameAdvantageOnBlock;
			}
			else
			{
				stunFrames = hit.frameAdvantageOnBlock + remainingFrames; //soonk - testing frame advantage for multi hit moves
			}
			//stunFrames = hit.frameAdvantageOnBlock + remainingFrames;
			if (stunFrames < 1) stunFrames = 1;
			stunTime = (Fix64)stunFrames / (Fix64)UFE.config.fps;
		}
		else if (hit.hitStunType == HitStunType.Frames)
		{
			stunFrames = (int)hit._hitStunOnBlock;
			if (stunFrames < 1) stunFrames = 1;
			stunTime = (Fix64)stunFrames / (Fix64)UFE.config.fps;
		}
		else
		{
			stunTime = hit._hitStunOnBlock;
		}

		UFE.FireBlock(myHitBoxesScript.GetStrokeHitBox(), opControlsScript.currentMove, this);

		HitTypeOptions hitEffects = UFE.config.blockOptions.blockHitEffects;
		Fix64 freezingTime = GetHitFreezingTime(hit.hitStrength);
		if (hit.overrideHitEffectsBlock)
		{
			hitEffects = hit.hitEffectsBlock;
			freezingTime = hitEffects._freezingTime;
		}

		// Create hit effect
		if (location.Length > 0 && hitEffects.hitParticle != null)
		{
			/*
            HitEffectSpawnPoint spawnPoint = hitEffects.spawnPoint;
            if (hit.overrideEffectSpawnPoint)
            {
                spawnPoint = hit.spawnPoint;
            }
            */
			//Vector3 newLocation = GetParticleSpawnPoint(spawnPoint, location);
			Vector3 newLocation = GetParticleSpawnPoint(hitEffects, location);

			long frames = Mathf.RoundToInt(hitEffects.killTime * UFE.config.fps);
			GameObject pTemp = UFE.SpawnGameObject(hitEffects.hitParticle, newLocation, Quaternion.identity, frames);

			//soonk - sticky particle test
			if (hitEffects.hitParticleSticky)
			{
				Transform targetTransform = myHitBoxesScript.GetTransform(hitEffects.bloodSpawnPoint.bloodBodyPart);
				pTemp.transform.SetParent(targetTransform);
				pTemp.transform.position = targetTransform.position;
				//if (particleEffect.particleEffect.followRotation) pTemp.AddComponent<StickyGameObject>();

			}

			if (hitEffects.mirrorOn2PSide && mirror > 0)
			{
				pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
			}

		}
		UFE.PlaySound(hitEffects.hitSound);


		// Shake Options
		shakeCamera = hitEffects.shakeCameraOnHit;
		shakeCharacter = hitEffects.shakeCharacterOnHit;
		shakeDensity = hitEffects._shakeDensity;
		shakeCameraDensity = hitEffects._shakeCameraDensity;

		//soonk addition
		if (currentSide == PossibleSides.P1)
		{
			//if (currentState == PossibleStates.Crouch)
			if (currentState == PossibleStates.Crouch || (currentState == PossibleStates.StandUp && downHeld))
			{
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingCrouchingHit, hit);
				currentHitInfo = myMoveSetScript.basicMoves.blockingCrouchingHit;

				if (!myMoveSetScript.AnimationExists(currentHitAnimation))
					Debug.LogError("Blocking Crouching Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");
			}
			//else if (currentState == PossibleStates.Stand)
			else if (currentState == PossibleStates.Stand || (currentState == PossibleStates.StandUp && !downHeld))
			{
				HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
				if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.blockingLowHit.animMap[0].clip != null)
				{
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingLowHit, hit);
					currentHitInfo = myMoveSetScript.basicMoves.blockingLowHit;
				}
				else
				{
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingHighHit, hit);
					currentHitInfo = myMoveSetScript.basicMoves.blockingHighHit;
					if (!myMoveSetScript.AnimationExists(currentHitAnimation))
						Debug.LogError("Blocking High Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");
				}
			}
			else if (!myPhysicsScript.IsGrounded())
			{
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingAirHit, hit);
				currentHitInfo = myMoveSetScript.basicMoves.blockingAirHit;
				if (!myMoveSetScript.AnimationExists(currentHitAnimation))
					Debug.LogError("Blocking Air Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");
			}
		}
		else
		{
			//if (currentState == PossibleStates.Crouch)
			if (currentState == PossibleStates.Crouch || (currentState == PossibleStates.StandUp && downHeld))
			{
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingCrouchingHitP2, hit);
				currentHitInfo = myMoveSetScript.basicMoves.blockingCrouchingHitP2;

				if (!myMoveSetScript.AnimationExists(currentHitAnimation))
					Debug.LogError("Blocking Crouching Hit P2 animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");
			}
			//else if (currentState == PossibleStates.Stand)
			else if (currentState == PossibleStates.Stand || (currentState == PossibleStates.StandUp && !downHeld))
			{
				HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
				if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.blockingLowHitP2.animMap[0].clip != null)
				{
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingLowHitP2, hit);
					currentHitInfo = myMoveSetScript.basicMoves.blockingLowHitP2;
				}
				else
				{
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingHighHitP2, hit);
					currentHitInfo = myMoveSetScript.basicMoves.blockingHighHitP2;
					if (!myMoveSetScript.AnimationExists(currentHitAnimation))
						Debug.LogError("Blocking High Hit P2 animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");
				}
			}
			else if (!myPhysicsScript.IsGrounded())
			{
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingAirHitP2, hit);
				currentHitInfo = myMoveSetScript.basicMoves.blockingAirHitP2;
				if (!myMoveSetScript.AnimationExists(currentHitAnimation))
					Debug.LogError("Blocking Air Hit P2 animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");
			}
		}

		myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation);
		hitAnimationSpeed = myMoveSetScript.GetAnimationLength(currentHitAnimation) / stunTime;

		if (currentHitInfo.autoSpeed)
		{
			myMoveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
		}

		// Freeze screen depending on how strong the hit was
		HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01);
		UFE.DelaySynchronizedAction(this.HitUnpause, freezingTime);

		// Reset hit to allow for another hit while the character is still stunned
		Fix64 spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.Small)
		{
			spaceBetweenHits = 1.1;
		}
		else if (hit.spaceBetweenHits == Sizes.Medium)
		{
			spaceBetweenHits = 1.3;
		}
		else if (hit.spaceBetweenHits == Sizes.High)
		{
			spaceBetweenHits = 1.7;
		}

		if (hitEffects.autoHitStop)
		{
			UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, freezingTime * spaceBetweenHits);
		}
		else
		{
			UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, hitEffects._hitStop * spaceBetweenHits);
		}

		// Add force to the move
		myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);

		if (!UFE.config.blockOptions.ignoreAppliedForceBlock)
			if (hit.applyDifferentBlockForce)
			{
				myPhysicsScript.AddForce(new FPVector(hit._pushForceBlock.x, hit._pushForceBlock.y, 0), ignoreDirection ? mirror : -opControlsScript.mirror);
			}
			else
			{
				myPhysicsScript.AddForce(new FPVector(hit._pushForce.x, 0, 0), ignoreDirection ? mirror : -opControlsScript.mirror);
			}
	}

	public void GetHit(Hit hit, int remainingFrames, FPVector[] location, bool ignoreDirection, ControlsScript attacker)
	{
		//soonk addition - punish text
		if (currentMove != null)
		{
			if (currentMove.moveConnected && currentMove.currentFrame >= currentMove.totalFrames - currentMove.recoveryFrames)
			{
				UFE.FireAlert(UFE.config.selectedLanguage.punish, opControlsScript);
			}
		}

		// Get what animation should be played depending on the character's state
		bool airHit = false;
		bool armored = false;
		bool isKnockDown = false;
		Fix64 damageModifier = 1;
		Fix64 hitStunModifier = 1;
		BasicMoveInfo currentHitInfo;

		currentHit = hit;

		myHitBoxesScript.isHit = true;

		if (myInfo.headLook.disableOnHit) ToggleHeadLook(false);

		if (currentMove != null && currentMove.frameLinks.Length > 0)
		{
			foreach (FrameLink frameLink in currentMove.frameLinks)
			{
				if (currentMove.currentFrame >= frameLink.activeFramesBegins &&
					currentMove.currentFrame <= frameLink.activeFramesEnds)
				{
					if (frameLink.linkType == LinkType.CounterMove)
					{
						bool cancelable = false;
						if (frameLink.counterMoveType == CounterMoveType.SpecificMove)
						{
							if (frameLink.counterMoveFilter == currentMove) cancelable = true;
						}
						else
						{
							HitBox strokeHitBox = myHitBoxesScript.GetStrokeHitBox();
							if ((frameLink.anyHitStrength || frameLink.hitStrength == hit.hitStrength) &&
								(frameLink.anyStrokeHitBox || frameLink.hitBoxType == strokeHitBox.type) &&
								(frameLink.anyHitType || frameLink.hitType == hit.hitType))
							{
								cancelable = true;
							}
						}

						if (cancelable)
						{
							frameLink.cancelable = true;
							if (frameLink.disableHitImpact)
							{
								Fix64 timeLeft = (Fix64)(currentMove.totalFrames - currentMove.currentFrame) / (Fix64)UFE.config.fps;

								myHitBoxesScript.ResetHit();
								UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, timeLeft);
								return;
							}
						}
					}
				}
			}
		}

		//soonk addition
		//moved from below hit reaction
		// Convert to percentage in case of DamageType
		Fix64 damage = hit._damageOnHit;
		if (hit.damageType == DamageType.Percentage) damage = myInfo.lifePoints * (damage / 100);
		// Damage deterioration
		if (hit.damageScaling)
		{
			if (UFE.config.comboOptions.damageDeterioration == Sizes.Small)
			{
				damage = damage - (damage * comboHits * .1);
			}
			else if (UFE.config.comboOptions.damageDeterioration == Sizes.Medium)
			{
				damage = damage - (damage * comboHits * .2);
			}
			else if (UFE.config.comboOptions.damageDeterioration == Sizes.High)
			{
				damage = damage - (damage * comboHits * .4);
			}
		}

		if (damage < UFE.config.comboOptions._minDamage) damage = UFE.config.comboOptions._minDamage;
		damage *= damageModifier;

		//soonk addition
		//rage damage
		if (inRage && myInfo.characterName != "Kyla")
		{
			damage *= 1.2f;
			//damage = damage *= 1.2f;
		}
		if (opControlsScript.inRage && opInfo.characterName != "Kyla")
		{
			damage *= 1.1f;
			//damage = damage *= 1.1f;
		}

		comboHitDamage = damage;
		comboDamage += damage;
		owner.comboHits++;

		if (comboHits > 1 && UFE.config.comboOptions.comboDisplayMode == ComboDisplayMode.ShowDuringComboExecution)
		{
			UFE.FireAlert(UFE.config.selectedLanguage.combo, opControlsScript);
		}

		// Lose life
		isDead = DamageMe(damage, hit.doesntKill);

		// Set position in case of pull enemy in
		activePullIn = null;
		if (hit.pullEnemyIn.enemyBodyPart != BodyPart.none && hit.pullEnemyIn.characterBodyPart != BodyPart.none)
		{
			FPVector newPos = myHitBoxesScript.GetPosition(hit.pullEnemyIn.enemyBodyPart);
			if (newPos != FPVector.zero)
			{
				activePullIn = new PullIn();
				activePullIn.position = worldTransform.position + (opControlsScript.HitBoxes.GetPosition(hit.pullEnemyIn.characterBodyPart) - newPos);
				activePullIn.speed = hit.pullEnemyIn.speed;
				activePullIn.forceStand = hit.pullEnemyIn.forceStand;
				activePullIn.position.z = 0;
				if (hit.pullEnemyIn.forceStand)
				{
					activePullIn.position.y = 0;
					myPhysicsScript.ForceGrounded();
				}
			}
		}

		if (hit.resetCrumples) consecutiveCrumple = 0;

		//soonk addition
		//lots of changes and doubled up for p2 side
		// Obtain animation depending on HitType
		//if (myPhysicsScript.IsGrounded()) {
		if (currentSide == PossibleSides.P1)
		{
			if (myPhysicsScript.IsGrounded() && (!isDead && !hitWhileDead))
			{
				if (hit.hitStrength == HitStrengh.Crumple && hit.hitType != HitType.Launcher)
				{
					if (myMoveSetScript.basicMoves.getHitCrumple.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Crumple animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitCrumple.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitCrumple;
					consecutiveCrumple++;

				}
				else if (hit.hitType == HitType.Launcher)
				{
					if (myMoveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Air Juggle animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitAir;

					airHit = true;
				}
				else if (hit.hitType == HitType.KnockBack)
				{
					if (myMoveSetScript.basicMoves.getHitKnockBack.animMap[0].clip == null)
					{
						if (myMoveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Air Juggle & Knock Back animations not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
						currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
					}
					else
					{
						currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
						currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
					}

					airHit = true;
				}
				else if (hit.hitType == HitType.HighKnockdown)
				{
					if (myMoveSetScript.basicMoves.getHitHighKnockdown.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Standing High Hit [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitHighKnockdown.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitHighKnockdown;

					isKnockDown = true;
				}
				else if (hit.hitType == HitType.MidKnockdown)
				{
					if (myMoveSetScript.basicMoves.getHitMidKnockdown.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Standing Mid Hit [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitMidKnockdown.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitMidKnockdown;

					isKnockDown = true;
				}
				else if (hit.hitType == HitType.Sweep)
				{
					if (myMoveSetScript.basicMoves.getHitSweep.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Sweep [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitSweep.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitSweep;

					isKnockDown = true;
				}
				else if (currentState == PossibleStates.Crouch && !hit.forceStand)
				{
					if (myMoveSetScript.basicMoves.getHitCrouching.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Crouching Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitCrouching, hit);
					currentHitInfo = myMoveSetScript.basicMoves.getHitCrouching;

				}
				else
				{
					HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
					if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.getHitLow.animMap[0].clip != null)
					{
						if (myMoveSetScript.basicMoves.getHitHigh.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Standing Low Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitLow, hit);
						currentHitInfo = myMoveSetScript.basicMoves.getHitLow;
					}
					else if (hit.hitType == HitType.RollLeft)
					{
						if (myMoveSetScript.basicMoves.getHitRollLeft.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Roll Left Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitRollLeft, hit);
						currentHitInfo = myMoveSetScript.basicMoves.getHitRollLeft;
					}
					else if (hit.hitType == HitType.RollRight)
					{
						if (myMoveSetScript.basicMoves.getHitRollRight.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Roll Right Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitRollRight, hit);
						currentHitInfo = myMoveSetScript.basicMoves.getHitRollRight;
					}
					else
					{
						if (myMoveSetScript.basicMoves.getHitHigh.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Standing High Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitHigh, hit);
						currentHitInfo = myMoveSetScript.basicMoves.getHitHigh;
					}
				}
			}
			else if (myPhysicsScript.IsGrounded() && isDead)
			{
				//soonk - added to force fall anim on any hit during final stun
				print("DEAD");
				if (hitWhileDead)
				{
					print("HWD");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitStunFall.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitStunFall;
				}
				else
				{
					if (currentLifePoints == 0 && opControlsScript.currentLifePoints == 0)
					{
						//draw = true;
					}
					print("NOT HWD");
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitStandToStun, hit);
					currentHitInfo = myMoveSetScript.basicMoves.getHitStandToStun;
				}
			}
			else
			{
				if (hit.hitStrength == HitStrengh.Crumple && myMoveSetScript.basicMoves.getHitKnockBack.animMap[0].clip != null)
				{
					currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
				}
				else
				{
					if (myMoveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Air Juggle animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
				}
				airHit = true;
			}
		}
		else
		{
			//soonk addition
			//p2 stuff
			if (myPhysicsScript.IsGrounded() && (!isDead && !hitWhileDead))
			{
				if (hit.hitStrength == HitStrengh.Crumple && hit.hitType != HitType.Launcher)
				{
					if (myMoveSetScript.basicMoves.getHitCrumpleP2.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Crumple animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitCrumpleP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitCrumpleP2;
					consecutiveCrumple++;

				}
				else if (hit.hitType == HitType.Launcher)
				{
					if (myMoveSetScript.basicMoves.getHitAirP2.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Air Juggle animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitAirP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitAirP2;

					airHit = true;
				}
				else if (hit.hitType == HitType.KnockBack)
				{
					if (myMoveSetScript.basicMoves.getHitKnockBackP2.animMap[0].clip == null)
					{
						if (myMoveSetScript.basicMoves.getHitAirP2.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Air Juggle & Knock Back animations not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = myMoveSetScript.basicMoves.getHitAirP2.name;
						currentHitInfo = myMoveSetScript.basicMoves.getHitAirP2;
					}
					else
					{
						currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBackP2.name;
						currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBackP2;
					}

					airHit = true;
				}
				else if (hit.hitType == HitType.HighKnockdown)
				{
					if (myMoveSetScript.basicMoves.getHitHighKnockdownP2.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Standing High Hit [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitHighKnockdownP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitHighKnockdownP2;

					isKnockDown = true;
				}
				else if (hit.hitType == HitType.MidKnockdown)
				{
					if (myMoveSetScript.basicMoves.getHitMidKnockdownP2.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Standing Mid Hit [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitMidKnockdownP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitMidKnockdownP2;

					isKnockDown = true;
				}
				else if (hit.hitType == HitType.Sweep)
				{
					if (myMoveSetScript.basicMoves.getHitSweepP2.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Sweep [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitSweepP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitSweepP2;

					isKnockDown = true;
				}
				else if (currentState == PossibleStates.Crouch && !hit.forceStand)
				{
					if (myMoveSetScript.basicMoves.getHitCrouchingP2.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Crouching Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitCrouchingP2, hit);
					currentHitInfo = myMoveSetScript.basicMoves.getHitCrouchingP2;

				}
				else
				{
					HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
					if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.getHitLowP2.animMap[0].clip != null)
					{
						if (myMoveSetScript.basicMoves.getHitHighP2.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Standing Low Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitLowP2, hit);
						currentHitInfo = myMoveSetScript.basicMoves.getHitLowP2;
					}
					else if (hit.hitType == HitType.RollLeft)
					{
						if (myMoveSetScript.basicMoves.getHitRollLeftP2.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Roll Left Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitRollLeftP2, hit);
						currentHitInfo = myMoveSetScript.basicMoves.getHitRollLeftP2;
					}
					else if (hit.hitType == HitType.RollRight)
					{
						if (myMoveSetScript.basicMoves.getHitRollRightP2.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Roll Right Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitRollRightP2, hit);
						currentHitInfo = myMoveSetScript.basicMoves.getHitRollRightP2;
					}
					else
					{
						if (myMoveSetScript.basicMoves.getHitHighP2.animMap[0].clip == null)
							Debug.LogError("(" + myInfo.characterName + ") Standing High Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
						currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitHighP2, hit);
						currentHitInfo = myMoveSetScript.basicMoves.getHitHighP2;
					}
				}
			}
			else if (myPhysicsScript.IsGrounded() && isDead)
			{
				//soonk - added to force fall anim on any hit during final stun
				print("DEAD");
				if (hitWhileDead)
				{
					print("HWD");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitStunFallP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitStunFallP2;
				}
				else
				{
					print("NOT HWD");
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitStandToStunP2, hit);
					currentHitInfo = myMoveSetScript.basicMoves.getHitStandToStunP2;
				}
			}
			else
			{
				if (hit.hitStrength == HitStrengh.Crumple && myMoveSetScript.basicMoves.getHitKnockBackP2.animMap[0].clip != null)
				{
					currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBackP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBackP2;
				}
				else
				{
					if (myMoveSetScript.basicMoves.getHitAirP2.animMap[0].clip == null)
						Debug.LogError("(" + myInfo.characterName + ") Air Juggle animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = myMoveSetScript.basicMoves.getHitAirP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitAirP2;
				}
				airHit = true;
			}
		}

		// Override Hit Animation
		myPhysicsScript.overrideStunAnimation = null;
		if (hit.overrideHitAnimation && myPhysicsScript.IsGrounded())
		{
			BasicMoveInfo basicMoveOverride = myMoveSetScript.GetBasicAnimationInfo(hit.newHitAnimation);
			if (basicMoveOverride != null)
			{
				currentHitInfo = basicMoveOverride;
				currentHitAnimation = currentHitInfo.name;
				myPhysicsScript.overrideStunAnimation = currentHitInfo;
			}
			else
			{
				Debug.LogWarning("(" + myInfo.characterName + ") " + currentHitAnimation + " animation not found! Override not applied.");
			}
		}

		// Obtain hit effects
		HitTypeOptions hitEffects = hit.hitEffects;
		if (!hit.overrideHitEffects)
		{
			if (hit.hitStrength == HitStrengh.Weak) hitEffects = UFE.config.hitOptions.weakHit;
			if (hit.hitStrength == HitStrengh.Medium) hitEffects = UFE.config.hitOptions.mediumHit;
			if (hit.hitStrength == HitStrengh.Heavy) hitEffects = UFE.config.hitOptions.heavyHit;
			if (hit.hitStrength == HitStrengh.Crumple) hitEffects = UFE.config.hitOptions.crumpleHit;
			if (hit.hitStrength == HitStrengh.Custom1) hitEffects = UFE.config.hitOptions.customHit1;
			if (hit.hitStrength == HitStrengh.Custom2) hitEffects = UFE.config.hitOptions.customHit2;
			if (hit.hitStrength == HitStrengh.Custom3) hitEffects = UFE.config.hitOptions.customHit3;
			if (hit.hitStrength == HitStrengh.Custom4) hitEffects = UFE.config.hitOptions.customHit4;
			if (hit.hitStrength == HitStrengh.Custom5) hitEffects = UFE.config.hitOptions.customHit5;
			if (hit.hitStrength == HitStrengh.Custom6) hitEffects = UFE.config.hitOptions.customHit6;
		}

		// Cancel current move if any
		if (!hit.armorBreaker && currentMove != null &&
			currentMove.armorOptions.hitsTaken < currentMove.armorOptions.hitAbsorption &&
			currentMove.currentFrame >= currentMove.armorOptions.activeFramesBegin &&
			currentMove.currentFrame <= currentMove.armorOptions.activeFramesEnds)
		{
			//soonk - armor flash stuff
			Material currentArmorMat = transform.GetComponentInChildren<SpriteRenderer>().material;
			armorColor = currentArmorMat.GetColor("_EmissionColor");
			armorColorStart = armorColor.a;
			armorHitFlashFade = currentMove.armorOptions.armorHitGlow / (currentMove.totalFrames - currentMove.currentFrame);
			armorFlash = currentMove.armorOptions.armorHitGlow;

			armored = true;
			currentMove.armorOptions.hitsTaken++;
			damageModifier -= currentMove.armorOptions.damageAbsorption * .01;
			if (currentMove.armorOptions.overrideHitEffects)
				hitEffects = currentMove.armorOptions.hitEffects;

		}
		else if (currentMove != null && !currentMove.hitAnimationOverride)
		{
			if ((UFE.config.counterHitOptions.startUpFrames && currentMove.currentFrameData == CurrentFrameData.StartupFrames) ||
				(UFE.config.counterHitOptions.activeFrames && currentMove.currentFrameData == CurrentFrameData.ActiveFrames) ||
				(UFE.config.counterHitOptions.recoveryFrames && currentMove.currentFrameData == CurrentFrameData.RecoveryFrames))
			{
				UFE.FireAlert(UFE.config.selectedLanguage.counterHit, opControlsScript);
				damageModifier += UFE.config.counterHitOptions._damageIncrease * .01;
				hitStunModifier += UFE.config.counterHitOptions._hitStunIncrease * .01;
			}

			CheckHits(currentMove, opControlsScript);
			foreach (ControlsScript assist in opControlsScript.assists) CheckHits(currentMove, assist);
			storedMove = null;

			KillCurrentMove();
		}

		// Create hit effect
		if (location.Length > 0 && hitEffects.hitParticle != null)
		{
			/*
            HitEffectSpawnPoint spawnPoint = hitEffects.spawnPoint;
            if (hit.overrideEffectSpawnPoint)
            {
                spawnPoint = hit.spawnPoint;
            }
            */
			//Vector3 newLocation = GetParticleSpawnPoint(spawnPoint, location);
			Vector3 newLocation = GetParticleSpawnPoint(hitEffects, location);

			long frames = Mathf.RoundToInt(hitEffects.killTime * UFE.config.fps);
			GameObject pTemp = UFE.SpawnGameObject(hitEffects.hitParticle, newLocation, Quaternion.identity, frames);

			//soonk - sticky particle test
			if (hitEffects.hitParticleSticky)
			{
				Transform targetTransform = myHitBoxesScript.GetTransform(hitEffects.bloodSpawnPoint.bloodBodyPart);
				pTemp.transform.SetParent(targetTransform);
				pTemp.transform.position = targetTransform.position;
				//if (particleEffect.particleEffect.followRotation) pTemp.AddComponent<StickyGameObject>();

			}

			if (hitEffects.mirrorOn2PSide && mirror > 0)
			{
				pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
			}
		}
		if (playerNum == 2)
		{
			UFE.p1InGameSaveInfo.gameStats["bloodSpilled"] += ((int)hitEffects.bloodSpillAmount * 100);
		}
		else
		{
			UFE.p2InGameSaveInfo.gameStats["bloodSpilled"] += ((int)hitEffects.bloodSpillAmount * 100);
		}


		// Play sound
		UFE.PlaySound(hitEffects.hitSound);

		// Shake Options
		shakeCamera = hitEffects.shakeCameraOnHit;
		shakeCharacter = hitEffects.shakeCharacterOnHit;
		shakeDensity = hitEffects._shakeDensity;
		shakeCameraDensity = hitEffects._shakeCameraDensity;

		// Cast First Hit if true
		if (!firstHit && !opControlsScript.firstHit)
		{
			opControlsScript.firstHit = true;
			//soonk addition
			opControlsScript.AddGauge(UFE.config.roundOptions.firstHitBonus, 0);
			opInfo.playerScore += 3000;
			//opControlsScript.AddGauge(UFE.config.roundOptions.firstHitBonus);
			UFE.FireAlert(UFE.config.selectedLanguage.firstHit, opControlsScript);
		}
		UFE.FireHit(myHitBoxesScript.GetStrokeHitBox(), opControlsScript.currentMove, opControlsScript);

		// Reset hit to allow for another hit while the character is still stunned
		Fix64 spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.Small)
		{
			spaceBetweenHits = 1.1;
		}
		else if (hit.spaceBetweenHits == Sizes.Medium)
		{
			spaceBetweenHits = 1.3;
		}
		else if (hit.spaceBetweenHits == Sizes.High)
		{
			spaceBetweenHits = 1.7;
		}

		if (hitEffects.autoHitStop)
		{
			UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, hitEffects._freezingTime * spaceBetweenHits);
		}
		else
		{
			UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, hitEffects._hitStop * spaceBetweenHits);
		}

		// Override Camera Speed
		if (hit.overrideCameraSpeed)
		{
			cameraScript.OverrideSpeed((float)hit._newMovementSpeed, (float)hit._newRotationSpeed);
			UFE.DelaySynchronizedAction(cameraScript.RestoreSpeed, hit._cameraSpeedDuration);
		}

		// Stun
		int stunFrames = 0;
		if ((currentMove == null || !currentMove.hitAnimationOverride) && (!armored || isDead))
		{
			// Hit stun deterioration (the longer the combo gets, the harder it is to combo)
			currentSubState = SubStates.Stunned;
			if (hit.hitStunType == HitStunType.FrameAdvantage)
			{
				stunFrames = hit.frameAdvantageOnHit + remainingFrames;
				if (stunFrames < 1) stunFrames = 1;
				if (stunFrames < UFE.config.comboOptions._minHitStun) stunFrames = UFE.config.comboOptions._minHitStun;
				stunTime = (Fix64)stunFrames / (Fix64)UFE.config.fps;
			}
			else if (hit.hitStunType == HitStunType.Frames)
			{
				stunFrames = (int)hit._hitStunOnHit;
				if (stunFrames < 1) stunFrames = 1;
				if (stunFrames < UFE.config.comboOptions._minHitStun) stunFrames = UFE.config.comboOptions._minHitStun;
				stunTime = (Fix64)stunFrames / (Fix64)UFE.config.fps;
			}
			else
			{
				stunFrames = (int)FPMath.Round(hit._hitStunOnHit * UFE.config.fps);
				stunTime = hit._hitStunOnHit;
			}

			if (UFE.config.characterRotationOptions.fixRotationOnHit) testCharacterRotation();

			if (!hit.resetPreviousHitStun)
			{
				if (UFE.config.comboOptions.hitStunDeterioration == Sizes.Small)
				{
					stunTime -= (Fix64)comboHits * .01;
				}
				else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.Medium)
				{
					stunTime -= (Fix64)comboHits * .02;
				}
				else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.High)
				{
					stunTime -= (Fix64)comboHits * .04;
				}
			}
			stunTime *= hitStunModifier;

			FPVector pushForce = new FPVector();
			if (!myPhysicsScript.IsGrounded() && hit.applyDifferentAirForce)
			{
				pushForce.x = hit._pushForceAir.x;
				pushForce.y = hit._pushForceAir.y;
			}
			else
			{
				pushForce.x = hit._pushForce.x;
				pushForce.y = hit._pushForce.y;
			}

			if (consecutiveCrumple > UFE.config.comboOptions.maxConsecutiveCrumple)
			{
				isKnockDown = true;
				airHit = true;
				pushForce.y = 1;
			}

			if (hit.overrideAirRecoveryType)
			{
				airRecoveryType = hit.newAirRecoveryType;
			}
			else
			{
				airRecoveryType = UFE.config.comboOptions.airRecoveryType;
			}

			// Add force to the move		
			// Air juggle deterioration (the longer the combo, the harder it is to push the opponent higher)
			if (pushForce.y > 0 || (isDead && !isKnockDown))
			{

				if (UFE.config.comboOptions.airJuggleDeteriorationType == AirJuggleDeteriorationType.ComboHits)
				{
					airJuggleHits = comboHits - 1;
				}
				if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.Small)
				{
					pushForce.y -= (pushForce.y * (Fix64)airJuggleHits * .04);
				}
				else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.Medium)
				{
					pushForce.y -= (pushForce.y * (Fix64)airJuggleHits * .1);
				}
				else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.High)
				{
					pushForce.y -= (pushForce.y * (Fix64)airJuggleHits * .3);
				}
				if (pushForce.y < UFE.config.comboOptions._minPushForce) pushForce.y = UFE.config.comboOptions._minPushForce;
				airJuggleHits++;
			}

			// Force a standard weight so the same air combo works on all characters
			if (UFE.config.comboOptions.fixJuggleWeight)
			{
				myPhysicsScript.ApplyNewWeight(UFE.config.comboOptions._juggleWeight);
			}
			if (hit.overrideJuggleWeight)
			{
				myPhysicsScript.ApplyNewWeight(hit._newJuggleWeight);
			}

			// Restand the opponent (or juggle) if its an OTG
			if (currentState == PossibleStates.Down)
			{
				if (pushForce.y > 0)
				{
					currentState = PossibleStates.NeutralJump;
				}
				else
				{
					currentState = PossibleStates.Stand;
				}
			}

			if (airHit && airRecoveryType == AirRecoveryType.CantMove && hit.instantAirRecovery)
				stunTime = 0.001;

			//soonk addition (removal)
			//if (isDead) stunTime = 99999;

			if ((airHit || (!myPhysicsScript.IsGrounded() && airRecoveryType == AirRecoveryType.DontRecover))
				&& pushForce.y > 0)
			{
				//soonk addition
				//doubled for p2 side
				if (currentSide == PossibleSides.P1)
				{
					if (myMoveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
						Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");
					//if (myMoveSetScript.basicMoves.getHitAir.invincible) myHitBoxesScript.HideHitBoxes(true);

					myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);
					myPhysicsScript.AddForce(new FPVector(pushForce.x, pushForce.y, 0), ignoreDirection ? mirror : -attacker.mirror);
					//soonk - removed to fix air hit issues
					/*
                    if (myMoveSetScript.basicMoves.getHitKnockBack.animMap[0].clip != null &&
                        pushForce.x > UFE.config.comboOptions._knockBackMinForce)
                    {
                        currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
                        currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
                    }
                    else
                    {
                        currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
                        currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
                    }
                    */
					currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitAir;

					if (hit.overrideHitAnimationBlend)
					{
						myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit._newHitBlendingIn, hit.resetHitAnimations);
					}
					else
					{
						myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit.resetHitAnimations);
					}

					if (currentHitInfo.autoSpeed)
					{
						// if the hit was in the air, calculate the time it will take for the character to hit the ground
						Fix64 airTime = myPhysicsScript.GetPossibleAirTime(pushForce.y);

						if (myMoveSetScript.basicMoves.fallingFromAirHit.animMap[0].clip == null) airTime *= 2;

						if (stunTime > airTime || airRecoveryType == AirRecoveryType.DontRecover)
						{
							stunTime = airTime;
						}

						myMoveSetScript.SetAnimationNormalizedSpeed(currentHitAnimation, (myMoveSetScript.GetAnimationLength(currentHitAnimation) / stunTime));
					}
				}
				else
				{
					if (myMoveSetScript.basicMoves.getHitAirP2.animMap[0].clip == null)
						Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air P2");
					//if (myMoveSetScript.basicMoves.getHitAir.invincible) myHitBoxesScript.HideHitBoxes(true);

					myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);

					myPhysicsScript.AddForce(new FPVector(pushForce.x, pushForce.y, 0), ignoreDirection ? mirror : -attacker.mirror);

					//soonk - not sure why this was in here, caused issues w/ air moves
					/*
                    if (myMoveSetScript.basicMoves.getHitKnockBackP2.animMap[0].clip != null &&
                        pushForce.x > UFE.config.comboOptions._knockBackMinForce)
                    {
                        currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBackP2.name;
                        currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBackP2;
                    }
                    else
                    {
                        currentHitAnimation = myMoveSetScript.basicMoves.getHitAirP2.name;
                        currentHitInfo = myMoveSetScript.basicMoves.getHitAirP2;
                    }
                    */
					currentHitAnimation = myMoveSetScript.basicMoves.getHitAirP2.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitAirP2;


					if (hit.overrideHitAnimationBlend)
					{
						myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit._newHitBlendingIn, hit.resetHitAnimations);
					}
					else
					{
						myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit.resetHitAnimations);
					}

					if (currentHitInfo.autoSpeed)
					{
						// if the hit was in the air, calculate the time it will take for the character to hit the ground
						Fix64 airTime = myPhysicsScript.GetPossibleAirTime(pushForce.y);

						if (myMoveSetScript.basicMoves.fallingFromAirHitP2.animMap[0].clip == null) airTime *= 2;

						if (stunTime > airTime || airRecoveryType == AirRecoveryType.DontRecover)
						{
							stunTime = airTime;
						}

						myMoveSetScript.SetAnimationNormalizedSpeed(currentHitAnimation, (myMoveSetScript.GetAnimationLength(currentHitAnimation) / stunTime));
					}
				}

				//end
			}
			else
			{
				hitAnimationSpeed = 0;
				//soonk addition
				//cut off ends below
				if (hit.hitType == HitType.HighKnockdown)
				{
					//applyKnockdownForces(UFE.config.knockDownOptions.high, attacker);
					myPhysicsScript.overrideAirAnimation = true;
					airRecoveryType = AirRecoveryType.DontRecover;
					if (!hit.customStunValues) stunTime =
						UFE.config.knockDownOptions.high._knockedOutTime;// + UFE.config.knockDownOptions.high._standUpTime;

				}
				else if (hit.hitType == HitType.MidKnockdown)
				{
					//applyKnockdownForces(UFE.config.knockDownOptions.highLow, attacker);
					myPhysicsScript.overrideAirAnimation = true;
					airRecoveryType = AirRecoveryType.DontRecover;
					if (!hit.customStunValues) stunTime =
						UFE.config.knockDownOptions.highLow._knockedOutTime;// + UFE.config.knockDownOptions.highLow._standUpTime;

				}
				else if (hit.hitType == HitType.Sweep)
				{
					applyKnockdownForces(UFE.config.knockDownOptions.sweep, attacker);
					myPhysicsScript.overrideAirAnimation = true;
					airRecoveryType = AirRecoveryType.DontRecover;
					if (!hit.customStunValues) stunTime =
						UFE.config.knockDownOptions.sweep._knockedOutTime;// + UFE.config.knockDownOptions.sweep._standUpTime;

				}

				hitAnimationSpeed = myMoveSetScript.GetAnimationLength(currentHitAnimation) / stunTime;

				if (hit.hitStrength == HitStrengh.Crumple)
				{
					stunTime += UFE.config.knockDownOptions.crumple._knockedOutTime;
				}

				//soonk - testing universal push stuff
				/*
                if (!myPhysicsScript.overrideAirAnimation)
                {
                    myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);
                    myPhysicsScript.AddForce(pushForce, ignoreDirection ? mirror : -attacker.mirror);
                }
                */
				myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);
				myPhysicsScript.AddForce(pushForce, ignoreDirection ? mirror : -attacker.mirror);

				// Set deceleration of hit stun animation so it can look more natural (deprecated)
				/*if (hit.overrideHitAcceleration) {
                    hitStunDeceleration = hitAnimationSpeed / 3;
                }*/

				if (hit.overrideHitAnimationBlend)
				{
					myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit._newHitBlendingIn, hit.resetHitAnimations);
				}
				else
				{
					myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit.resetHitAnimations);
				}

				if (currentHitInfo.autoSpeed && hitAnimationSpeed > 0)
				{
					myMoveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
				}

			}
		}

		//soonk addition
		//NCFA set
		ncfa_active = hit.ncfa;
		// Freeze screen depending on how strong the hit was
		HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01);
		if (myPhysicsScript.IsGrounded())
		{
			UFE.DelaySynchronizedAction(this.HitUnpause, hitEffects._freezingTime);
		}
		else
		{
			UFE.DelaySynchronizedAction(this.HitUnpause, hitEffects._freezingTimeAirborne);
		}
		//UFE.DelaySynchronizedAction(this.HitUnpause, hitEffects._freezingTime);
	}

	private Vector3 GetParticleSpawnPoint(HitEffectSpawnPoint spawnPoint, FPVector[] locations)
	{
		if (spawnPoint == HitEffectSpawnPoint.StrikingHurtBox)
		{
			return locations[0].ToVector();
		}
		else if (spawnPoint == HitEffectSpawnPoint.StrokeHitBox)
		{
			return locations[1].ToVector();
			//soonk addition
			//body part spawn
		}
		else if (spawnPoint == HitEffectSpawnPoint.Bodypart)
		{
			//FPVector bodyPartLocation = myHitBoxesScript.GetPosition(currentHit.bloodSpawnPoint.bloodBodyPart);
			FPVector bodyPartLocation = myHitBoxesScript.GetPosition(currentHit.hitEffects.bloodSpawnPoint.bloodBodyPart);
			return bodyPartLocation.ToVector();
		}
		else
		{
			return locations[2].ToVector();
		}
	}

	private Vector3 GetParticleSpawnPoint(HitTypeOptions hitEffects, FPVector[] locations)
	{
		HitEffectSpawnPoint spawnPoint = hitEffects.spawnPoint;
		if (spawnPoint == HitEffectSpawnPoint.StrikingHurtBox)
		{
			return locations[0].ToVector();
		}
		else if (spawnPoint == HitEffectSpawnPoint.StrokeHitBox)
		{
			return locations[1].ToVector();
			//soonk addition
			//body part spawn
		}
		else if (spawnPoint == HitEffectSpawnPoint.Bodypart)
		{
			//FPVector bodyPartLocation = myHitBoxesScript.GetPosition(currentHit.bloodSpawnPoint.bloodBodyPart);
			FPVector bodyPartLocation = myHitBoxesScript.GetPosition(hitEffects.bloodSpawnPoint.bloodBodyPart);
			return bodyPartLocation.ToVector();
		}
		else
		{
			return locations[2].ToVector();
		}
	}

	private void applyKnockdownForces(SubKnockdownOptions knockdownOptions, ControlsScript attacker)
	{
		myPhysicsScript.ResetForces(true, true);
		myPhysicsScript.AddForce(knockdownOptions._predefinedPushForce, -attacker.mirror);
	}

	private string GetHitAnimation(BasicMoveInfo hitMove, Hit hit)
	{
		if (hit.hitStrength == HitStrengh.Weak) return hitMove.name;
		if (hitMove.animMap[1].clip != null && hit.hitStrength == HitStrengh.Medium) return myMoveSetScript.GetAnimationString(hitMove, 2);
		if (hitMove.animMap[2].clip != null && hit.hitStrength == HitStrengh.Heavy) return myMoveSetScript.GetAnimationString(hitMove, 3);
		if (hitMove.animMap[3].clip != null && hit.hitStrength == HitStrengh.Custom1) return myMoveSetScript.GetAnimationString(hitMove, 4);
		if (hitMove.animMap[4].clip != null && hit.hitStrength == HitStrengh.Custom2) return myMoveSetScript.GetAnimationString(hitMove, 5);
		if (hitMove.animMap[5].clip != null && hit.hitStrength == HitStrengh.Custom3) return myMoveSetScript.GetAnimationString(hitMove, 6);
		if (hitMove.animMap.Length > 6 && hitMove.animMap[6].clip != null && hit.hitStrength == HitStrengh.Custom4) return myMoveSetScript.GetAnimationString(hitMove, 7);
		if (hitMove.animMap.Length > 7 && hitMove.animMap[7].clip != null && hit.hitStrength == HitStrengh.Custom5) return myMoveSetScript.GetAnimationString(hitMove, 8);
		if (hitMove.animMap.Length > 8 && hitMove.animMap[8].clip != null && hit.hitStrength == HitStrengh.Custom6) return myMoveSetScript.GetAnimationString(hitMove, 9);
		return hitMove.name;
	}

	public void ToggleHeadLook(bool flag)
	{
		if (headLookScript != null && myInfo.headLook.enabled) headLookScript.enabled = flag;
	}

	// Pause animations and physics to create a sense of impact
	public void HitPause()
	{
		HitPause(0);
	}

	public void HitPause(Fix64 animSpeed)
	{
		//soonk addition (removal)
		//forced camera shake
		//if (shakeCamera) Camera.main.transform.position += Vector3.forward/2;
		myPhysicsScript.freeze = true;

		PausePlayAnimation(true, animSpeed);
	}

	// Unpauses the pause
	public void HitUnpause()
	{
		if (cameraScript.cinematicFreeze) return;
		myPhysicsScript.freeze = false;

		PausePlayAnimation(false);
	}

	// Method to pause animations and return them to their prior speed accordly

	private void PausePlayAnimation(bool pause)
	{
		PausePlayAnimation(pause, 0);
	}

	private void PausePlayAnimation(bool pause, Fix64 animSpeed)
	{
		if (animSpeed < 0) animSpeed = 0;
		if (pause)
		{
			myMoveSetScript.SetAnimationSpeed(animSpeed);
		}
		else
		{
			myMoveSetScript.RestoreAnimationSpeed();
		}
	}

	public void AddGauge(Fix64 gaugeGain, int targetGauge)
	{
		if ((isDead || opControlsScript.isDead) && UFE.config.roundOptions.inhibitGaugeGain) return;
		if (!UFE.config.gameGUI.hasGauge) return;
		if (inhibitGainWhileDraining) return;
		currentGaugesPoints[targetGauge] += (myInfo.maxGaugePoints * (gaugeGain / 100));
		if (currentGaugesPoints[targetGauge] > myInfo.maxGaugePoints) currentGaugesPoints[targetGauge] = myInfo.maxGaugePoints;
		if (gaugeGain < 0)
		{
			if (currentGaugesPoints[targetGauge] < 0) currentGaugesPoints[targetGauge] = 0;
		}
	}
	public void AddGaugeConnect(Fix64 gaugeGain, int targetGauge)
	{
		if ((isDead || opControlsScript.isDead) && UFE.config.roundOptions.inhibitGaugeGain) return;
		if (!UFE.config.gameGUI.hasGauge) return;
		//if (inhibitGainWhileDraining) return;
		//int oldGauge = (int)currentGaugesPoints[targetGauge];
		currentGaugesPoints[targetGauge] += gaugeGain;
		//currentGaugesPoints[targetGauge] += (myInfo.maxGaugePoints * (gaugeGain / 100));
		if (currentGaugesPoints[targetGauge] > myInfo.maxGaugePoints) currentGaugesPoints[targetGauge] = myInfo.maxGaugePoints;
		if (gaugeGain < 0)
		{
			if (currentGaugesPoints[targetGauge] < 0) currentGaugesPoints[targetGauge] = 0;
		}
		if (moveOnConnect != null && currentGaugesPoints[targetGauge] <= 0)
		{
			storedMove = moveOnConnect;
		}
		//if (moveOnConnect != null && oldGauge > 0 && currentGaugesPoints[targetGauge] == 0) CastMove(moveOnConnect, true);
	}

	private void RemoveGauge(Fix64 gaugeLoss, int targetGauge)
	{
		if ((isDead || opControlsScript.isDead) && UFE.config.roundOptions.inhibitGaugeGain) return;
		if (!UFE.config.gameGUI.hasGauge) return;
		if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
			&& playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Infinite) return;
		if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
			&& playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Infinite) return;
		currentGaugesPoints[targetGauge] -= (myInfo.maxGaugePoints * (gaugeLoss / 100));
		if (currentGaugesPoints[targetGauge] < 0) currentGaugesPoints[targetGauge] = 0;

		if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
			&& ((playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Refill)
			|| (playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Refill)))
		{
			if (!UFE.FindAndUpdateDelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime))
				UFE.DelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime);
		}
	}

	public bool DamageMe(Fix64 damage, bool doesntKill)
	{
		if (doesntKill && damage >= currentLifePoints) damage = currentLifePoints - 1;
		return DamageMe(damage);
	}

	private void RefillLife()
	{
		currentLifePoints = myInfo.lifePoints;
		UFE.FireLifePoints(myInfo.lifePoints, opControlsScript);
	}

	private void RefillGauge()
	{
		for (int i = 0; i < currentGaugesPoints.Length; i++) AddGauge(myInfo.maxGaugePoints, i);
	}

	//soonk addition
	//2 functions
	private IEnumerator<float> _FinishRound()
	{
		timeOutDead = true;
		BasicMoveInfo stc = currentSide == PossibleSides.P1 ? myMoveSetScript.basicMoves.getHitStandToStun : myMoveSetScript.basicMoves.getHitStandToStunP2;
		myMoveSetScript.PlayBasicMove(stc, false);
		float stcTime = (float)myMoveSetScript.GetAnimationLength(stc.name) / (float)myMoveSetScript.GetAnimationSpeed(stc.name);
		yield return Timing.WaitForSeconds(stcTime);
		stc = currentSide == PossibleSides.P1 ? myMoveSetScript.basicMoves.stunIdle : myMoveSetScript.basicMoves.stunIdleP2;
		myMoveSetScript.PlayBasicMove(stc, true);
		yield break;
	}

	public void FinishRound()
	{
		isDead = DamageMe(1200);
		Timing.RunCoroutine(_FinishRound());
	}

	private bool DamageMe(Fix64 damage)
	{
		if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
			&& playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Infinite) return false;
		if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
			&& playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Infinite) return false;
		if (currentLifePoints <= 0) return true;
		//soonk addition (removal)
		//if (UFE.GetTimer() <= 0 && UFE.config.roundOptions.hasTimer) return true;

		currentLifePoints -= damage;
		//soonk addition
		//add to score based on damage
		if (UFE.timer > 0) opInfo.playerScore += ((int)damage * 100);

		if (currentLifePoints < 0) currentLifePoints = 0;

		if (currentLifePoints == 0)
		{
			if (!myPhysicsScript.IsGrounded())
			{

				currentLifePoints = 1;
				tempDead = true;
			}
		}

		UFE.FireLifePoints(currentLifePoints, opControlsScript);

		if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
			&& ((playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Refill)
			|| (playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Refill)))
		{
			if (currentLifePoints == 0) currentLifePoints = myInfo.lifePoints;
			if (!UFE.FindAndUpdateDelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime))
			{
				UFE.DelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime);
			}
		}

		if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
			&& playerNum == 1 && UFE.config.trainingModeOptions.p1Life != LifeBarTrainingMode.Normal) return false;
		if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
			&& playerNum == 2 && UFE.config.trainingModeOptions.p2Life != LifeBarTrainingMode.Normal) return false;

		if (currentLifePoints == 0)
		{
			return true;
		}


		/*
        if (currentLifePoints == 0)
        {
            //soonk addition
            //end of round stuff
            defaultBattleGUI.FinishingTime();
            Timing.RunCoroutine(_EndRoundCountdown(), "Countdown");

            if (UFE.config.roundOptions.slowMotionKO)
            {
                UFE.timeScale = UFE.timeScale * UFE.config.roundOptions._slowMoSpeed;
                UFE.DelaySynchronizedAction(this.ReturnTimeScale, UFE.config.roundOptions._slowMoTimer);
            }
            else
            {
                Timing.RunCoroutine(_CheckForFinisher(), Segment.SlowUpdate, "Finisher");
            }

            return true;
        }
        */
		return false;
	}

	public void DrawReset()
	{
		print("Draw");
		currentLifePoints = 300;
		draw = false;
		currentState = PossibleStates.Stand;
		currentSubState = SubStates.Resting;
		if (currentSide == PossibleSides.P1)
		{
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
		}
		else
		{
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idleP2);
		}
		stunTime = 0;

	}

	//soonk addition
	//3 functions
	//soonk - end of round stuff
	public void EndRoundFunctions()
	{
		if (currentLifePoints == 0 && opControlsScript.currentLifePoints == 0)
		{
			draw = true;
			opControlsScript.draw = true;
			print("DDDD2");
			myPhysicsScript.AddForce(new FPVector(15, 0, 0), -opControlsScript.mirror);
			//DrawReset();
			//opControlsScript.DrawReset();
			Timing.KillAllCoroutines();
			UFE.fluxCapacitor.EndRound();
			//ResetData(false);
			//UFE.DelaySynchronizedAction(UFE.fluxCapacitor.EndRound, .5f);
		}
		else
		{
			Debug.Log("End Round Function");
			//soonk addition
			//end of round stuff
			defaultBattleGUI.FinishingTime();
			Timing.RunCoroutine(_EndRoundCountdown(), "Countdown");

			if (UFE.config.roundOptions.slowMotionKO)
			{
				UFE.timeScale = UFE.timeScale * UFE.config.roundOptions._slowMoSpeed;
				UFE.DelaySynchronizedAction(this.ReturnTimeScale, UFE.config.roundOptions._slowMoTimer);
			}
			else
			{
				Timing.RunCoroutine(_CheckForFinisher(), Segment.SlowUpdate, "Finisher");
			}
		}

	}
	public void EndRoundFunctionsDraw()
	{
		print("End of round function DRAW");
		defaultBattleGUI.DrawTime();
		myPhysicsScript.AddForce(new FPVector(10, 0, 0), -opControlsScript.mirror);
		opControlsScript.myPhysicsScript.AddForce(new FPVector(-10, 0, 0), -opControlsScript.mirror);
		DrawReset();
		opControlsScript.DrawReset();
		draw = true;
		this.StartNewRound();
		/*
        draw = true;
        opControlsScript.draw = true;
        print("End of round function DRAW");
        myPhysicsScript.AddForce(new FPVector(15, 0, 0), -opControlsScript.mirror);
        //DrawReset();
        //opControlsScript.DrawReset();
        Timing.KillAllCoroutines();
        UFE.fluxCapacitor.EndRound();*/

	}

	private IEnumerator<float> _EndRoundCountdown()
	{
		Debug.Log("EOR Countdown");
		while (true)
		{
			yield return Timing.WaitForSeconds(5f);
			Debug.Log("EOR Countdown Ended");
			Timing.KillAllCoroutines();
			//Timing.KillCoroutines("Countdown");
			if (!firstFinisherStarted)
			{
				UFE.DelaySynchronizedAction(this.EndRound, .5f);
				UFE.DelaySynchronizedAction(UFE.fluxCapacitor.EndRound, .5f);
			}
		}
	}

	private IEnumerator<float> _CheckForFinisher()
	{
		Debug.Log("Check For Finisher");
		while (true)
		{
			//yield return Timing.WaitForSeconds (5f);
			if (hitWhileDead)
			{
				Debug.Log("Finisher Canceled by Hit");
				Timing.KillAllCoroutines();
				Timing.RunCoroutine(defaultBattleGUI._TimerBarFade());
				defaultBattleGUI.stopSound();
				//Timing.KillCoroutines("Finisher");
				//Timing.KillCoroutines("Countdown");
				this.EndRound();
				UFE.fluxCapacitor.EndRound();
				/*
                UFE.DelaySynchronizedAction(this.EndRound, .5f);
                UFE.DelaySynchronizedAction(UFE.fluxCapacitor.EndRound, .5f);
                */
			}
			yield return Timing.WaitForSeconds(.5f);
		}
	}

	private void ReturnTimeScale()
	{
		UFE.timeScale = UFE.config._gameSpeed;
		UFE.DelaySynchronizedAction(this.EndRound, (Fix64)2);
	}

	private void StartNextChallenge()
	{
		UFE.config.lockInputs = true;
		UFE.config.lockMovements = true;
		UFE.DelaySynchronizedAction(UFE.StartFight, (Fix64)2);

		if (challengeMode.resetRound)
		{
			UFE.ResetTimer();

			ResetData(true);
			opControlsScript.ResetData(false);
		}

		challengeMode.Run();
	}

	public void SetMoveToOutro()
	{
		if (finisherEnded || opControlsScript.finisherEnded)
		{
			UFE.DelaySynchronizedAction(this.TakeEndScreenshot, .1f);
			outroPlayed = true;
			UFE.outroPlayed = true;
		}
		else
		{
			this.SetMove(myMoveSetScript.GetOutro());
			if (currentMove != null)
			{
				Debug.Log("Set Move To Outro");
				currentMove.currentFrame = 0;
				currentMove.currentTick = 0;
				//soonk addition
				UFE.DelaySynchronizedAction(this.TakeEndScreenshot, 90);
			}
			outroPlayed = true;
			UFE.outroPlayed = true;
		}
	}

	//soonk addition
	//2 functions
	public void TakeEndScreenshot()
	{
		if (Application.isEditor)
		{
		}
		else
		{
			SaveUSBData();
		}
		StartCoroutine(_TestScreenshot());
	}

	IEnumerator _TestScreenshot()
	{
		yield return new WaitForEndOfFrame();
		Texture2D endGameSS = new Texture2D(Screen.width, Screen.height);
		endGameSS.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		endGameSS.Apply();

		defaultBattleGUI.endImage(endGameSS);
		Timing.RunCoroutine(defaultBattleGUI._EndScreenFade());

		Debug.Log("_TestScreenshot");
		GameObject.Destroy(UFE.overlayCam);


		yield return 0;
	}

	public void SaveUSBData()
	{
		StageVisitP1();
		StageVisitP2();
		//only the player that wins runs this script, check which one
		//if it's p1
		if (playerNum == 1)
		{
			Debug.Log("P1 won, saving USB");
			// Check achievements
			// beat someone w/ 50 ranked wins
			if (UFE.p2InGameSaveInfo.gameStats["pvpWins"] >= 50) UFE.p1InGameSaveInfo.achievementsEarned[0] = true;
			// lose to mind master
			if (myInfo.characterName == "Mind Master") UFE.p2InGameSaveInfo.achievementsEarned[1] = true;
			// win w/ 10% health
			if (currentLifePoints / myInfo.lifePoints <= .1) UFE.p1InGameSaveInfo.achievementsEarned[2] = true;
			// first win
			if (UFE.p1InGameSaveInfo.gameStats["pvpWins"] == 0 && UFE.p1InGameSaveInfo.gameStats["cpuWins"] == 0) UFE.p1InGameSaveInfo.achievementsEarned[5] = true;
			// 250 wins
			if (UFE.p1InGameSaveInfo.gameStats["pvpWins"] + UFE.p1InGameSaveInfo.gameStats["cpuWins"] == 249) UFE.p1InGameSaveInfo.achievementsEarned[7] = true;
			// 100 wins
			if (UFE.p1InGameSaveInfo.gameStats["pvpWins"] + UFE.p1InGameSaveInfo.gameStats["cpuWins"] == 99) UFE.p1InGameSaveInfo.achievementsEarned[8] = true;
			// 500 wins
			if (UFE.p1InGameSaveInfo.gameStats["pvpWins"] + UFE.p1InGameSaveInfo.gameStats["cpuWins"] == 499) UFE.p1InGameSaveInfo.achievementsEarned[14] = true;
			// spill 100 gallons of blood
			if (UFE.p1InGameSaveInfo.gameStats["bloodSpilled"] >= 100) UFE.p1InGameSaveInfo.achievementsEarned[19] = true;
			// 10 wall bounces
			if (UFE.p1InGameSaveInfo.gameStats["wallBouncesCaused"] >= 10) UFE.p1InGameSaveInfo.achievementsEarned[16] = true;
			// win on the anniversary of release
			if (System.DateTime.Today.Month == 8 && System.DateTime.Today.Day == 15) UFE.p1InGameSaveInfo.achievementsEarned[37] = true;
			// beat a dev
			if (UFE.p2InGameSaveInfo.devUSB) UFE.p1InGameSaveInfo.achievementsEarned[62] = true;
			// spill 250 gallons of blood
			if (UFE.p1InGameSaveInfo.gameStats["bloodSpilled"] >= 250) UFE.p1InGameSaveInfo.achievementsEarned[40] = true;
			// 10 throws
			if (UFE.p1InGameSaveInfo.gameStats["timesSideSwitched"] >= 10) UFE.p1InGameSaveInfo.achievementsEarned[41] = true;
			// 50 wins
			if (UFE.p1InGameSaveInfo.gameStats["pvpWins"] + UFE.p1InGameSaveInfo.gameStats["cpuWins"] == 49) UFE.p1InGameSaveInfo.achievementsEarned[46] = true;
			// 5 hit combo
			if (UFE.p1InGameSaveInfo.gameStats["mostComboHits"] >= 5) UFE.p1InGameSaveInfo.achievementsEarned[48] = true;
			// defeat Mind Master
			if (opInfo.characterName == "Mind Master") UFE.p1InGameSaveInfo.achievementsEarned[50] = true;
			// 10 hit combo
			if (UFE.p1InGameSaveInfo.gameStats["mostComboHits"] >= 10) UFE.p1InGameSaveInfo.achievementsEarned[54] = true;
			// first opponent loss
			if (UFE.p2InGameSaveInfo.gameStats["pvpLosses"] == 0) UFE.p1InGameSaveInfo.achievementsEarned[55] = true;
			// 50 crouch attacks
			if (UFE.p1InGameSaveInfo.gameStats["crouchAttackCount"] >= 50) UFE.p1InGameSaveInfo.achievementsEarned[64] = true;
			// 10 wins
			if (UFE.p1InGameSaveInfo.gameStats["pvpWins"] + UFE.p1InGameSaveInfo.gameStats["cpuWins"] == 9) UFE.p1InGameSaveInfo.achievementsEarned[82] = true;
			// 10 pvp wins in a row
			if (UFE.p1InGameSaveInfo.gameStats["pvpWinStreak"] == 9 && UFE.gameMode == GameMode.VersusMode) UFE.p1InGameSaveInfo.achievementsEarned[89] = true;
			// 250 damage combo
			if (UFE.p1InGameSaveInfo.gameStats["highestComboDmg"] >= 250) UFE.p1InGameSaveInfo.achievementsEarned[26] = true;
			// jump check
			if (!hasJumped) UFE.p1InGameSaveInfo.achievementsEarned[96] = true;
			// block check
			if (!hasBlocked) UFE.p1InGameSaveInfo.achievementsEarned[29] = true;
			// 50 sweeps
			if (UFE.p1InGameSaveInfo.gameStats["sweepAttackCount"] >= 50) UFE.p1InGameSaveInfo.achievementsEarned[30] = true;
			// 10 throw techs
			if (UFE.p1InGameSaveInfo.gameStats["timesTechedThrow"] >= 10) UFE.p1InGameSaveInfo.achievementsEarned[4] = true;
			// find sakata
			if (opInfo.characterName == "Sakata") UFE.p1InGameSaveInfo.achievementsEarned[74] = true;
			// 10 anti airs
			if (UFE.p1InGameSaveInfo.gameStats["aaAttackCount"] >= 10) UFE.p1InGameSaveInfo.achievementsEarned[70] = true;
			// check move types for kick/arm/left/right achievements
			moveTypes.Distinct().ToList();
			// right attacks only
			if (!moveTypes.Contains("LeftArm") && !moveTypes.Contains("LeftLeg")) UFE.p1InGameSaveInfo.achievementsEarned[72] = true;
			// left attacks only
			if (!moveTypes.Contains("RightArm") && !moveTypes.Contains("RightLeg")) UFE.p1InGameSaveInfo.achievementsEarned[6] = true;
			// arms only
			if (!moveTypes.Contains("RightLeg") && !moveTypes.Contains("LeftLeg")) UFE.p1InGameSaveInfo.achievementsEarned[58] = true;
			// legs only
			if (!moveTypes.Contains("RightArm") && !moveTypes.Contains("LeftArm")) UFE.p1InGameSaveInfo.achievementsEarned[42] = true;



			// save char choice
			UFE.p1InGameSaveInfo.charStats[myInfo.characterName + "Selected"]++;
			UFE.p2InGameSaveInfo.charStats[opInfo.characterName + "Selected"]++;
			//save perfect
			if (currentLifePoints == myInfo.lifePoints)
			{
				UFE.p1InGameSaveInfo.gameStats["perfectWins"] += 1;
				// perfect win
				UFE.p1InGameSaveInfo.achievementsEarned[3] = true;
				// perfect against sakata
				if (opInfo.characterName == "Sakata") UFE.p1InGameSaveInfo.achievementsEarned[11] = true;
			}
			//if the game was vs a real opponent, save pvp wins and add to the pvp streak
			if (UFE.gameMode == GameMode.VersusMode)
			{
				UFE.p1InGameSaveInfo.gameStats["pvpWins"] += 1;
				UFE.p1InGameSaveInfo.gameStats["pvpWinStreak"] += 1;
				UFE.p1InGameSaveInfo.gameStats["lastPVPmatchWon"] = 1;
				UFE.p2InGameSaveInfo.gameStats["pvpWinStreak"] = 0;
				UFE.p2InGameSaveInfo.gameStats["lastPVPmatchWon"] = 0;
			}
			// otherwise save cpu wins and add to the beat cpu stats for this character
			else
			{
				UFE.p1InGameSaveInfo.gameStats["cpuWins"] += 1;
				UFE.p1InGameSaveInfo.charStats[myInfo.characterName + "BeatCPU"]++;
				UFE.p2InGameSaveInfo.charStats[opInfo.characterName + "LostCPU"]++;
			}
		}
		//if it's p2
		else
		{
			Debug.Log("P2 won, saving USB");
			// save char choice
			UFE.p2InGameSaveInfo.charStats[myInfo.characterName + "Selected"]++;
			UFE.p1InGameSaveInfo.charStats[opInfo.characterName + "Selected"]++;
			//save perfect
			if (currentLifePoints == myInfo.lifePoints) UFE.p2InGameSaveInfo.gameStats["perfectWins"] += 1;
			//if the game was vs a real opponent, save pvp wins and add to the pvp streak
			if (UFE.gameMode == GameMode.VersusMode)
			{
				UFE.p2InGameSaveInfo.gameStats["pvpWins"] += 1;
				UFE.p2InGameSaveInfo.gameStats["pvpWinStreak"] += 1;
				UFE.p2InGameSaveInfo.gameStats["lastPVPmatchWon"] = 1;
				UFE.p1InGameSaveInfo.gameStats["pvpWinStreak"] = 0;
				UFE.p1InGameSaveInfo.gameStats["lastPVPmatchWon"] = 0;
			}
			// otherwise save cpu wins and add to the beat cpu stats for this character
			else
			{
				UFE.p2InGameSaveInfo.gameStats["cpuWins"] += 1;
				UFE.p2InGameSaveInfo.charStats[myInfo.characterName + "BeatCPU"]++;
				UFE.p1InGameSaveInfo.charStats[opInfo.characterName + "LostCPU"]++;
			}
		}
		// save info
		UFE.usbHandler.SaveBothUSB();
	}

	// character finisher tracking
	public static void CharacterSingleFinishersP1(string charName)
	{
		switch (charName)
		{
			case "Veil":
				UFE.p1InGameSaveInfo.achievementsEarned[24] = true;
				break;
			case "Kyla":
				UFE.p1InGameSaveInfo.achievementsEarned[81] = true;
				break;
			case "Titan":
				UFE.p1InGameSaveInfo.achievementsEarned[57] = true;
				break;
			case "Wilson":
				UFE.p1InGameSaveInfo.achievementsEarned[45] = true;
				break;
			case "Trenton":
				UFE.p1InGameSaveInfo.achievementsEarned[91] = true;
				break;
			case "Ravona":
				UFE.p1InGameSaveInfo.achievementsEarned[49] = true;
				break;
			case "Vamphyrial":
				UFE.p1InGameSaveInfo.achievementsEarned[33] = true;
				break;
			case "Kin Kade":
				UFE.p1InGameSaveInfo.achievementsEarned[51] = true;
				break;
			default:
				break;
		}
	}
	public static void CharacterCompleteFinishersP1(string charName)
	{
		switch (charName)
		{
			case "Veil":
				UFE.p1InGameSaveInfo.achievementsEarned[99] = true;
				break;
			case "Kyla":
				UFE.p1InGameSaveInfo.achievementsEarned[34] = true;
				break;
			case "Titan":
				UFE.p1InGameSaveInfo.achievementsEarned[27] = true;
				break;
			case "Wilson":
				UFE.p1InGameSaveInfo.achievementsEarned[73] = true;
				break;
			case "Trenton":
				UFE.p1InGameSaveInfo.achievementsEarned[56] = true;
				break;
			case "Ravona":
				UFE.p1InGameSaveInfo.achievementsEarned[67] = true;
				break;
			case "Vamphyrial":
				UFE.p1InGameSaveInfo.achievementsEarned[79] = true;
				break;
			case "Kin Kade":
				UFE.p1InGameSaveInfo.achievementsEarned[80] = true;
				break;
			default:
				break;
		}
	}
	// check for visiting stages
	public static void StageVisitP1()
	{
		// checking for one of each stage
		for (int i = 0; i < UFE.config.characters.Length; i++)
		{
			if (UFE.config.selectedStage.stageName.Contains(UFE.config.characters[i].characterName))
			{
				UFE.p1InGameSaveInfo.stageVisitsList.Add(UFE.config.characters[i].characterName);
				if (!UFE.p1InGameSaveInfo.stageVisitsList.Contains(UFE.config.characters[i].characterName))
				{
					UFE.p1InGameSaveInfo.stageVisitsList.Add(UFE.config.characters[i].characterName);
				}
			}
		}
		if (UFE.p1InGameSaveInfo.stageVisitsList.Count >= 8)
		{
			UFE.p1InGameSaveInfo.achievementsEarned[76] = true;
		}
		// checking all stages
		UFE.p1InGameSaveInfo.stageVariationsList.Add(UFE.config.selectedStage.stageName);
		List<string> tempStageList = UFE.p1InGameSaveInfo.stageVariationsList.Distinct().ToList();
		UFE.p1InGameSaveInfo.stageVariationsList = tempStageList.ToList();
		if (UFE.p1InGameSaveInfo.stageVariationsList.Count >= UFE.config.stages.Length)
		{
			UFE.p1InGameSaveInfo.achievementsEarned[75] = true;
		}
	}
	public static void StageVisitP2()
	{
		// checking for one of each stage
		for (int i = 0; i < UFE.config.characters.Length; i++)
		{
			if (UFE.config.selectedStage.stageName.Contains(UFE.config.characters[i].characterName))
			{
				UFE.p2InGameSaveInfo.stageVisitsList.Add(UFE.config.characters[i].characterName);
				if (!UFE.p2InGameSaveInfo.stageVisitsList.Contains(UFE.config.characters[i].characterName))
				{
					UFE.p2InGameSaveInfo.stageVisitsList.Add(UFE.config.characters[i].characterName);
				}
			}
		}
		if (UFE.p2InGameSaveInfo.stageVisitsList.Count >= 8)
		{
			UFE.p2InGameSaveInfo.achievementsEarned[76] = true;
		}
		// checking all stages
		UFE.p2InGameSaveInfo.stageVariationsList.Add(UFE.config.selectedStage.stageName);
		List<string> tempStageList = UFE.p2InGameSaveInfo.stageVariationsList.Distinct().ToList();
		UFE.p2InGameSaveInfo.stageVariationsList = tempStageList.ToList();
		if (UFE.p2InGameSaveInfo.stageVariationsList.Count >= UFE.config.stages.Length)
		{
			UFE.p2InGameSaveInfo.achievementsEarned[75] = true;
		}
	}

	public void ResetData(bool resetLife)
	{
		if (UFE.config.roundOptions.resetPositions)
		{
			if (playerNum == 1)
			{
				worldTransform.position = new FPVector(UFE.config.roundOptions._p1XPosition, 0, worldTransform.position.z);
			}
			else
			{
				worldTransform.position = new FPVector(UFE.config.roundOptions._p2XPosition, 0, worldTransform.position.z);
			}
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle, myMoveSetScript.basicMoves.idle.name, 0);
			myPhysicsScript.ForceGrounded();

			currentState = PossibleStates.Stand;
			currentSubState = SubStates.Resting;
			stunTime = 0;

		}
		else if (currentState == PossibleStates.Down && myPhysicsScript.IsGrounded())
		{
			stunTime = 1;
		}
		/*
        if (draw)
        {
            print("Draw");
            currentLifePoints = 300;
            draw = false;
            currentState = PossibleStates.Stand;
            currentSubState = SubStates.Resting;
            if (currentSide == PossibleSides.P1)
            {
                myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
            }
            else
            {
                myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idleP2);
            }
            stunTime = 0;
        }
        */
		else if (resetLife || UFE.config.roundOptions.resetLifePoints)
		{
			if (playerNum == 1 && (UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode))
			{
				currentLifePoints = (Fix64)myInfo.lifePoints * (UFE.config.trainingModeOptions.p1StartingLife / 100);
			}
			else if (playerNum == 2 && (UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode))
			{
				currentLifePoints = (Fix64)myInfo.lifePoints * (UFE.config.trainingModeOptions.p2StartingLife / 100);
			}
			else
			{
				currentLifePoints = myInfo.lifePoints;
			}
		}

		blockStunned = false;
		comboHits = 0;
		comboDamage = 0;
		comboHitDamage = 0;
		airJuggleHits = 0;
		CheckBlocking(false);
		isDead = false;
		tempDead = false;
		myPhysicsScript.isTakingOff = false;
		myPhysicsScript.isLanding = false;

		myPhysicsScript.ResetWeight();
		ToggleHeadLook(true);
	}

	// Get amount of freezing time depending on the Strengtht of the move
	public Fix64 GetHitAnimationSpeed(HitStrengh hitStrength)
	{
		if (hitStrength == HitStrengh.Weak)
		{
			return UFE.config.hitOptions.weakHit._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Medium)
		{
			return UFE.config.hitOptions.mediumHit._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Heavy)
		{
			return UFE.config.hitOptions.heavyHit._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Crumple)
		{
			return UFE.config.hitOptions.crumpleHit._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Custom1)
		{
			return UFE.config.hitOptions.customHit1._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Custom2)
		{
			return UFE.config.hitOptions.customHit2._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Custom3)
		{
			return UFE.config.hitOptions.customHit3._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Custom4)
		{
			return UFE.config.hitOptions.customHit4._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Custom5)
		{
			return UFE.config.hitOptions.customHit5._animationSpeed;
		}
		else if (hitStrength == HitStrengh.Custom6)
		{
			return UFE.config.hitOptions.customHit6._animationSpeed;
		}
		return 0;
	}

	// Get amount of freezing time depending on the Strengtht of the move
	public Fix64 GetHitFreezingTime(HitStrengh hitStrength)
	{
		if (hitStrength == HitStrengh.Weak)
		{
			return UFE.config.hitOptions.weakHit._freezingTime;
		}
		else if (hitStrength == HitStrengh.Medium)
		{
			return UFE.config.hitOptions.mediumHit._freezingTime;
		}
		else if (hitStrength == HitStrengh.Heavy)
		{
			return UFE.config.hitOptions.heavyHit._freezingTime;
		}
		else if (hitStrength == HitStrengh.Crumple)
		{
			return UFE.config.hitOptions.crumpleHit._freezingTime;
		}
		else if (hitStrength == HitStrengh.Custom1)
		{
			return UFE.config.hitOptions.customHit1._freezingTime;
		}
		else if (hitStrength == HitStrengh.Custom2)
		{
			return UFE.config.hitOptions.customHit2._freezingTime;
		}
		else if (hitStrength == HitStrengh.Custom3)
		{
			return UFE.config.hitOptions.customHit3._freezingTime;
		}
		else if (hitStrength == HitStrengh.Custom4)
		{
			return UFE.config.hitOptions.customHit4._freezingTime;
		}
		else if (hitStrength == HitStrengh.Custom5)
		{
			return UFE.config.hitOptions.customHit5._freezingTime;
		}
		else if (hitStrength == HitStrengh.Custom6)
		{
			return UFE.config.hitOptions.customHit6._freezingTime;
		}
		return 0;
	}

	// Shake Camera
	void shakeCam()
	{
		float rnd = Random.Range((float)shakeCameraDensity * -.1f, (float)shakeCameraDensity * .1f);
		Camera.main.transform.position += new Vector3(rnd, rnd, 0);
	}

	// Shake Character while being hit and in freezing mode
	void shake()
	{
		Fix64 rnd = FPRandom.Range((float)shakeDensity * -.1f, (float)shakeDensity * .1f);
		localTransform.position = new FPVector(localTransform.position.x + rnd, localTransform.position.y, localTransform.position.z);
	}

	public void SetActive(bool value)
	{
		gameObject.SetActive(value);
	}

	public bool GetActive()
	{
		return gameObject.activeInHierarchy;
	}

	//soonk addition
	public void StartNewRound()
	{
		print("Start New Round");
		Timing.KillAllCoroutines();
		if (draw)
		{
			print("1234");
			ResetData(false);
		}
		else
		{
			defaultBattleGUI.ResetRound();
		}
		isDead = false;
		tempDead = false;
		opControlsScript.isDead = false;
		opControlsScript.tempDead = false;
		timeOutDead = false;
		opControlsScript.timeOutDead = false;
		hitWhileDead = false;
		opControlsScript.hitWhileDead = false;
		currentGaugesPoints[0] = 0;
		opControlsScript.currentGaugesPoints[0] = 0;
		Camera overlayCam = UFE.overlayCam.GetComponent<Camera>();
		//overlayCam.enabled = false;
		finisherEnded = false;
		//finisherPlayer = UFE.overlayCam.GetComponent<VideoPlayer>();
	}
	public void EndRound()
	{
		//Timing.KillAllCoroutines();
		//soonk - stun stuff
		if (isDead && !hitWhileDead)
		{
			Debug.Log("End of round dead stuff");
			hitWhileDead = true;
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.getHitStunFall, false);
		}
	}

}