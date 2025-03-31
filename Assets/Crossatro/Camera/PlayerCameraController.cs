using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] 
    private CrossWordGridGenerator m_crossGridGenerator;

    [SerializeField] 
    private Transform m_cameraTarget;

    [SerializeField] 
    private AnimationCurve m_cameraSpeedRatioByMousePositionRatioCurve;

    [SerializeField] 
    private float m_moveTargetCameraSpeed = 10.0f;

    private Vector2 m_mousePosition;
    
    public event Action<Vector3> OnTargetCameraMove;
    private void Awake()
    {
        m_crossGridGenerator.OnEndGridGeneration += OnGenerateGridCallback;
    }

    private void OnGenerateGridCallback(CrossWordsGameGrid crossWordsGameGrid)
    {
        List<GridWord> listNonValidatedWord = crossWordsGameGrid.GetAllNonValidatedWords();
        Vector2 gridMiddle = crossWordsGameGrid.GetMiddleWordList(listNonValidatedWord);
        m_cameraTarget.position = new Vector3(gridMiddle.x, 0, gridMiddle.y);
        OnTargetCameraMove?.Invoke(m_cameraTarget.transform.position);
    }

    private void Update()
    {
        MoveTargetCamera();
    }

    private void MoveTargetCamera()
    {
        if (m_mousePosition.x <= 0 || m_mousePosition.x > Screen.width || m_mousePosition.y <= 0 || m_mousePosition.y > Screen.height)
        {
            return;
        }
        
        float mousePositionXRatio = Mathf.InverseLerp(0, Screen.width, m_mousePosition.x);
        float mousePositionYRatio = Mathf.InverseLerp(0, Screen.height, m_mousePosition.y);

        float mouseSpeedRatioX = m_cameraSpeedRatioByMousePositionRatioCurve.Evaluate(mousePositionXRatio);
        float mouseSpeedRatioY = m_cameraSpeedRatioByMousePositionRatioCurve.Evaluate(mousePositionYRatio);

        Vector2 targetCameraSpeed = new Vector2(mouseSpeedRatioX * m_moveTargetCameraSpeed,
            mouseSpeedRatioY * m_moveTargetCameraSpeed);
        m_cameraTarget.position += m_cameraTarget.forward * targetCameraSpeed.y * Time.deltaTime ;
        m_cameraTarget.position += m_cameraTarget.right * targetCameraSpeed.x * Time.deltaTime ;
        if (targetCameraSpeed.x ==0 && targetCameraSpeed.y == 0)
        {
            return;
        }
        
        OnTargetCameraMove?.Invoke(m_cameraTarget.transform.position);
    }

    public void OnMouseMoveCallback(InputAction.CallbackContext context)
    {
        m_mousePosition = context.ReadValue<Vector2>();
        
    }
}
