using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance; // �̱��� �ν��Ͻ�

    private Camera mainCamera; // ��鸱 ���� ī�޶�
    private Vector3 originalCameraPos; // ī�޶� �ʱ� ��ġ ���� ����

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // �ν��Ͻ� ����
        }
        else
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� �����ϸ� ���� ��ü�� �ı�
            return;
        }

        mainCamera = Camera.main; // "MainCamera" �±װ� ���� ī�޶� �ڵ����� ã��
        if (mainCamera != null)
        {
            originalCameraPos = mainCamera.transform.position; // �ʱ� ��ġ ����
        }
        else
        {
            Debug.LogError("Main camera not found. Please tag the main camera as 'MainCamera'.");
        }
    }

    [SerializeField]
    [Range(0.1f, 0.5f)]
    private float shakeRange = 0.5f; // ��鸲 ����

    [SerializeField]
    [Range(0.1f, 1f)]
    private float duration = 0.1f; // ��鸲 ���� �ð�

    public void Shake()
    {
        if (mainCamera != null)
        {
            StopAllCoroutines(); // �ٸ� ��鸲�� ���� ���̸� ����
            StartCoroutine(ShakeCoroutine());
        }
        else
        {
            Debug.LogError("Main camera is not assigned.");
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float CameraPosX = Random.value * shakeRange * 2 - shakeRange;
            float CameraPosY = Random.value * shakeRange * 2 - shakeRange;

            Vector3 newCameraPos = originalCameraPos;
            newCameraPos.x += CameraPosX;
            newCameraPos.y += CameraPosY;

            mainCamera.transform.position = newCameraPos; // ī�޶� ��ġ ����

            elapsed += Time.deltaTime;

            yield return null;
        }

        mainCamera.transform.position = originalCameraPos; // �ʱ� ��ġ�� ����
    }
}
