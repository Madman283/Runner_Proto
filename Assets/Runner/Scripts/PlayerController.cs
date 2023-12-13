using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HyperCasual.Runner
{
    /// <summary>
    /// A class used to control a player in a Runner
    /// game. Includes logic for player movement as well as 
    /// other gameplay logic.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary> Returns the PlayerController. </summary>
        public static PlayerController Instance => s_Instance;
        static PlayerController s_Instance;

        private CapsuleCollider player_collider;

        private float original_player_collider_height;

        [SerializeField]
        Animator m_Animator;

        [SerializeField]
        SkinnedMeshRenderer m_SkinnedMeshRenderer;

        [SerializeField]
        PlayerSpeedPreset m_PlayerSpeed = PlayerSpeedPreset.Medium;

        [SerializeField]
        float m_CustomPlayerSpeed = 10.0f;

        [SerializeField]
        float m_AccelerationSpeed = 10.0f;

        [SerializeField]
        float m_DecelerationSpeed = 20.0f;

        [SerializeField]
        float m_HorizontalSpeedFactor = 0.5f;

        [SerializeField]
        float m_ScaleVelocity = 2.0f;

        [SerializeField]
        bool m_AutoMoveForward = true;

        Vector3 m_LastPosition;
        float m_StartHeight;

        const float k_MinimumScale = 0.1f;
        static readonly string s_Speed = "Speed";

        Player_action action_type = Player_action.NONE;

        enum Player_action
        {
            NONE,
            JUMP_UP,
            JUMP_DOWN,
            SLIDE
        }

        Player_lane_change change_lane = Player_lane_change.NONE;

        enum Player_lane_change
        {
            NONE,
            RIGHT,
            LEFT
        }

        private float Switch_lanes_smoother = .01F;

        private float[] running_lanes = { 0, 1, 2, 3, 4 };

        public float distance_betwean_lanes = 1;

        protected float start_lane;

        private int current_lane = 2;

        public float jump_force_up = 6;

        public float jump_force_down = 8;

        public float jump_peak_height = 10;

        private float original_Y_start;

        private float slide_last_position;

        public float slide_distance; //distance of the slide

        private Vector3 original_collieder_size; 



        enum PlayerSpeedPreset
        {
            Slow,
            Medium,
            Fast,
            Custom
        }

        Transform m_Transform;
        Vector3 m_StartPosition;
        bool m_HasInput;
        float m_MaxXPosition;
        float m_XPos;
        float m_YPos;
        float m_ZPos;
        float m_TargetPosition;
        float m_Speed;
        float m_TargetSpeed;
        Vector3 m_Scale;
        Vector3 m_TargetScale;
        Vector3 m_DefaultScale;

        const float k_HalfWidth = 0.5f;

        /// <summary> The player's root Transform component. </summary>
        public Transform Transform => m_Transform;

        /// <summary> The player's current speed. </summary>
        public float Speed => m_Speed;

        /// <summary> The player's target speed. </summary>
        public float TargetSpeed => m_TargetSpeed;

        /// <summary> The player's minimum possible local scale. </summary>
        public float MinimumScale => k_MinimumScale;

        /// <summary> The player's current local scale. </summary>
        public Vector3 Scale => m_Scale;

        /// <summary> The player's target local scale. </summary>
        public Vector3 TargetScale => m_TargetScale;

        /// <summary> The player's default local scale. </summary>
        public Vector3 DefaultScale => m_DefaultScale;

        /// <summary> The player's default local height. </summary>
        public float StartHeight => m_StartHeight;

        /// <summary> The player's default local height. </summary>
        public float TargetPosition => m_TargetPosition;

        /// <summary> The player's maximum X position. </summary>
        public float MaxXPosition => m_MaxXPosition;

        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;

            Initialize();
        }

        public void Lane_set_up()
        {
            running_lanes[3] = start_lane + distance_betwean_lanes;
            running_lanes[4] = start_lane + (distance_betwean_lanes * 2);
            running_lanes[1] = start_lane - distance_betwean_lanes;
            running_lanes[0] = start_lane - (distance_betwean_lanes * 2);
        }

        /// <summary>
        /// Set up all necessary values for the PlayerController.
        /// </summary>
        public void Initialize()
        {
            start_lane = this.transform.position.x;
            running_lanes[2] = start_lane;
            current_lane = 2;
            original_Y_start = this.transform.position.y;
            m_Transform = transform;
            m_StartPosition = m_Transform.position;
            m_DefaultScale = m_Transform.localScale;
            m_Scale = m_DefaultScale;
            m_TargetScale = m_Scale;

            player_collider = this.GetComponent<CapsuleCollider>();
            original_player_collider_height = player_collider.height;

            Lane_set_up();

            if (m_SkinnedMeshRenderer != null)
            {
                m_StartHeight = m_SkinnedMeshRenderer.bounds.size.y;
            }
            else 
            {
                m_StartHeight = 1.0f;
            }

            ResetSpeed();
        }

        /// <summary>
        /// Returns the current default speed based on the currently
        /// selected PlayerSpeed preset.
        /// </summary>
        public float GetDefaultSpeed()
        {
            switch (m_PlayerSpeed)
            {
                case PlayerSpeedPreset.Slow:
                    return 5.0f;

                case PlayerSpeedPreset.Medium:
                    return 10.0f;

                case PlayerSpeedPreset.Fast:
                    return 20.0f;
            }

            return m_CustomPlayerSpeed;
        }

        /// <summary>
        /// Adjust the player's current speed
        /// </summary>
        public void AdjustSpeed(float speed)
        {
            m_TargetSpeed += speed;
            m_TargetSpeed = Mathf.Max(0.0f, m_TargetSpeed);
        }

        /// <summary>
        /// Reset the player's current speed to their default speed
        /// </summary>
        public void ResetSpeed()
        {
            m_Speed = 0.0f;
            m_TargetSpeed = GetDefaultSpeed();
        }

        /// <summary>
        /// Adjust the player's current scale
        /// </summary>
        public void AdjustScale(float scale)
        {
            m_TargetScale += Vector3.one * scale;
            m_TargetScale = Vector3.Max(m_TargetScale, Vector3.one * k_MinimumScale);
        }

        /// <summary>
        /// Reset the player's current speed to their default speed
        /// </summary>
        public void ResetScale()
        {
            m_Scale = m_DefaultScale;
            m_TargetScale = m_DefaultScale;
        }

        /// <summary>
        /// Returns the player's transform component
        /// </summary>
        public Vector3 GetPlayerTop()
        {
            return m_Transform.position + Vector3.up * (m_StartHeight * m_Scale.y - m_StartHeight);
        }

        /// <summary>
        /// Sets the target X position of the player
        /// </summary>
        public void SetDeltaPosition(float normalizedDeltaPosition, float jump_or_slide)
        {
            if (m_MaxXPosition == 0.0f)
            {
                Debug.LogError("Player cannot move because SetMaxXPosition has never been called or Level Width is 0. If you are in the LevelEditor scene, ensure a level has been loaded in the LevelEditor Window!");
            }

            if (jump_or_slide > 0 && action_type == Player_action.NONE)
            {
                action_type = Player_action.JUMP_UP;
            }
            else if (jump_or_slide < 0 && action_type == Player_action.NONE)
            {
                //Debug.Log("slide");
                slide_last_position = this.transform.position.z;
                action_type = Player_action.SLIDE;
            }

            if(normalizedDeltaPosition < 0 && change_lane == Player_lane_change.NONE && current_lane < 4)
            {
                //Debug.Log(current_lane + " R");
                change_lane = Player_lane_change.RIGHT;
            }
            else if(normalizedDeltaPosition > 0 && change_lane == Player_lane_change.NONE && current_lane > 0)
            {
                //Debug.Log(current_lane + " L");
                change_lane = Player_lane_change.LEFT;
            }
            
            //free movement
            /*float fullWidth = m_MaxXPosition * 2.0f;
            m_TargetPosition = m_TargetPosition + fullWidth * normalizedDeltaPosition;
            m_TargetPosition = Mathf.Clamp(m_TargetPosition, -m_MaxXPosition, m_MaxXPosition);
            m_HasInput = true;*/
        }

        /// <summary>
        /// Stops player movement
        /// </summary>
        public void CancelMovement()
        {
            m_HasInput = false;
        }

        /// <summary>
        /// Set the level width to keep the player constrained
        /// </summary>
        public void SetMaxXPosition(float levelWidth)
        {
            // Level is centered at X = 0, so the maximum player
            // X position is half of the level width
            m_MaxXPosition = levelWidth * k_HalfWidth;
        }

        /// <summary>
        /// Returns player to their starting position
        /// </summary>
        public void ResetPlayer()
        {
            m_Transform.position = m_StartPosition;
            m_XPos = 0.0f;
            m_ZPos = m_StartPosition.z;
            m_TargetPosition = 0.0f;

            m_LastPosition = m_Transform.position;

            m_HasInput = false;

            ResetSpeed();
            ResetScale();
        }

        public void Jump_up_player()
        {


            if (this.transform.position.y >= original_Y_start + (jump_peak_height - 1))
            {
                action_type = Player_action.JUMP_DOWN;
            }
            else
            {
                float new_Y_postiontion_target = Mathf.Lerp(m_YPos, jump_peak_height, jump_force_up * Time.deltaTime);
                float new_y_position_difference = new_Y_postiontion_target - m_YPos;


                m_YPos += new_y_position_difference;
            }
        }

        public void Jump_down_player()
        {
            if (this.transform.position.y <= (original_Y_start + 1))
            {
                action_type = Player_action.NONE;
            }
            else
            {
                float new_Y_postiontion_target = Mathf.Lerp(m_YPos, original_Y_start, jump_force_down * Time.deltaTime);
                float new_y_position_difference = new_Y_postiontion_target - m_YPos;


                m_YPos += new_y_position_difference;
            }
        }

        public void Slide_player_start()
        {
            
            if(this.transform.position.z > slide_distance + slide_last_position )
            {
                player_collider.height = original_player_collider_height;
            }
            else if(player_collider.height >= original_player_collider_height - 1)
            {
                player_collider.height = original_player_collider_height / 4;
            }
        }

        public void left(int lane)
        {
            if (this.transform.position.x <= (running_lanes[lane] + Switch_lanes_smoother))
            {
                
                current_lane = current_lane - 1;
                change_lane = Player_lane_change.NONE;
            }
            else
            {
                float new_x_positiontarget = Mathf.Lerp(m_XPos, running_lanes[lane], Speed * Time.deltaTime);
                float new_x_positiondifference = new_x_positiontarget - m_XPos;

                m_XPos += new_x_positiondifference;
            }
        }

        public void right(int lane)
        {
            if (this.transform.position.x >= (running_lanes[lane] - Switch_lanes_smoother))
            {
                current_lane = current_lane + 1;
                change_lane = Player_lane_change.NONE;
            }
            else
            {
                float new_x_positiontarget = Mathf.Lerp(m_XPos, running_lanes[lane], Speed * Time.deltaTime);
                float new_x_positiondifference = new_x_positiontarget - m_XPos;

                m_XPos += new_x_positiondifference;
            }
        }


        void Update()
        {
            float deltaTime = Time.deltaTime;

            //jump
            if(action_type == Player_action.JUMP_UP)
            {
                Jump_up_player();
            }
            else if(action_type == Player_action.JUMP_DOWN)
            {
                Jump_down_player();
            }
            else if(action_type == Player_action.SLIDE)
            {
                Slide_player_start();
            }
            else if (action_type == Player_action.NONE)
            {
                m_YPos = m_Transform.position.y;
            }

            //jump
            if (change_lane == Player_lane_change.RIGHT)
            {
                right(current_lane + 1);
                //Debug.Log(current_lane + " R");
            }
            else if (change_lane == Player_lane_change.LEFT)
            {
                left(current_lane - 1);
                //Debug.Log(current_lane + " L");
            }
            else if (change_lane == Player_lane_change.NONE)
            {
                m_XPos = running_lanes[current_lane];
            }

            // Update Scale

            if (!Approximately(m_Transform.localScale, m_TargetScale))
            {
                m_Scale = Vector3.Lerp(m_Scale, m_TargetScale, deltaTime * m_ScaleVelocity);
                m_Transform.localScale = m_Scale;
            }

            // Update Speed

            if (!m_AutoMoveForward && !m_HasInput)
            {
                Decelerate(deltaTime, 0.0f);
            }
            else if (m_TargetSpeed < m_Speed)
            {
                Decelerate(deltaTime, m_TargetSpeed);
            }
            else if (m_TargetSpeed > m_Speed)
            {
                Accelerate(deltaTime, m_TargetSpeed);
            }

            float speed = m_Speed * deltaTime;

            // Update position

            m_ZPos += speed;


            //this is used for the more free movemnt stile (non-lane change)
            /*if (m_HasInput)
            {
                float horizontalSpeed = speed * m_HorizontalSpeedFactor;

                float newPositionTarget = Mathf.Lerp(m_XPos, m_TargetPosition, horizontalSpeed);
                float newPositionDifference = newPositionTarget - m_XPos;

                newPositionDifference = Mathf.Clamp(newPositionDifference, -horizontalSpeed, horizontalSpeed);

                m_XPos += newPositionDifference;
            }*/

            m_Transform.position = new Vector3(m_XPos, /*m_Transform.position.y*/ m_YPos, m_ZPos);

            if (m_Animator != null && deltaTime > 0.0f)
            {
                float distanceTravelledSinceLastFrame = (m_Transform.position - m_LastPosition).magnitude;
                float distancePerSecond = distanceTravelledSinceLastFrame / deltaTime;

                m_Animator.SetFloat(s_Speed, distancePerSecond);
            }

            if (m_Transform.position != m_LastPosition)
            {
                m_Transform.forward = Vector3.Lerp(m_Transform.forward, (m_Transform.position - m_LastPosition).normalized, speed);
            }

            m_LastPosition = m_Transform.position;
        }

        void Accelerate(float deltaTime, float targetSpeed)
        {
            m_Speed += deltaTime * m_AccelerationSpeed;
            m_Speed = Mathf.Min(m_Speed, targetSpeed);
        }

        void Decelerate(float deltaTime, float targetSpeed)
        {
            m_Speed -= deltaTime * m_DecelerationSpeed;
            m_Speed = Mathf.Max(m_Speed, targetSpeed);
        }

        bool Approximately(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
        }
    }
}