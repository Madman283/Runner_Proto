using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace HyperCasual.Runner
{
    /// <summary>
    /// A simple Input Manager for a Runner game.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// Returns the InputManager.
        /// </summary>
        public static InputManager Instance => s_Instance;
        static InputManager s_Instance;

        [SerializeField]
        float m_InputSensitivity = 1.5f;

        bool m_HasInput;
        Vector3 m_InputPosition;
        Vector3 m_PreviousInputPosition;

        float dis_from = 10;//distrance of movment for a jump/slide action to happen


        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
        }

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        void Update()
        {
            if (PlayerController.Instance == null)
            {
                return;
            }

#if UNITY_EDITOR
            m_InputPosition = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.isPressed)
            {
                if (!m_HasInput)
                {
                    m_PreviousInputPosition = m_InputPosition;
                }
                m_HasInput = true;
            }
            else
            {
                m_HasInput = false;
            }
#else
            if (Touch.activeTouches.Count > 0)
            {
                m_InputPosition = Touch.activeTouches[0].screenPosition;

                if (!m_HasInput)
                {
                    m_PreviousInputPosition = m_InputPosition;
                }
                
                m_HasInput = true;
            }
            else
            {
                m_HasInput = false;
            }
#endif

            if (m_HasInput)
            {
                //float normalizedDeltaPosition = (m_InputPosition.x - m_PreviousInputPosition.x) / Screen.width * m_InputSensitivity;

                float jump_or_slide;

                float left_or_right; //lane change

                if (m_InputPosition.y > m_PreviousInputPosition.y + dis_from) // jump
                {
                    jump_or_slide = 1;
                }
                else if (m_InputPosition.y < m_PreviousInputPosition.y - dis_from) //slide
                {
                    jump_or_slide = -1;
                }
                else
                {
                    jump_or_slide = 0;
                }


                if(m_InputPosition.x > m_PreviousInputPosition.x + dis_from * 2) //right
                {
                    left_or_right = 1;
                }
                else if(m_InputPosition.x < m_PreviousInputPosition.x - dis_from * 2) //left
                {
                    left_or_right = -1;
                }
                else
                {
                    left_or_right = 0;
                }

                PlayerController.Instance.SetDeltaPosition(/*normalizedDeltaPosition*/left_or_right, jump_or_slide);
            }
            else
            {
                PlayerController.Instance.CancelMovement();
            }

            m_PreviousInputPosition = m_InputPosition;
        }
    }
}

