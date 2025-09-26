
using UnityEngine;

public class WheelchairManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject wheelchairPrefab;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float fallHeightThreshold = -5f;

    private GameObject currentWheelchair;
    private WheelchairController currentController;
    private bool isEjected = false; // Controle de estado para ejeção

    void Start()
    {
        // Configura a cadeira inicial, se existir
        currentWheelchair = FindObjectOfType<WheelchairController>()?.gameObject;

        if (currentWheelchair != null)
        {
            currentController = currentWheelchair.GetComponent<WheelchairController>();
            currentController.manager = this;
        }
        else
        {
            SpawnWheelchair();
        }
    }

    void Update()
    {
        // Verifica se a cadeira caiu abaixo do limite de altura
        if (currentWheelchair != null &&
            currentWheelchair.transform.position.y < fallHeightThreshold &&
            !isEjected)
        {
            isEjected = true; // Marca como ejetado
            currentController.EjectRider();
        }
    }

    public void SpawnWheelchair()
    {
        // Garante que o wheelchair seja instanciado apenas no ponto de respawn
        Vector3 spawnPos = respawnPoint ? respawnPoint.position : Vector3.zero;
        Quaternion spawnRot = respawnPoint ? respawnPoint.rotation : Quaternion.identity;

        currentWheelchair = Instantiate(wheelchairPrefab, spawnPos, spawnRot);
        currentController = currentWheelchair.GetComponent<WheelchairController>();
        currentController.manager = this;

        isEjected = false; // Reseta o estado de ejeção
    }

    public void RespawnWheelchair()
    {
        // Destroi a instância atual e cria uma nova no ponto de respawn
        if (currentWheelchair != null)
        {
            Destroy(currentWheelchair);
        }
        SpawnWheelchair();
    }
}