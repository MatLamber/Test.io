using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

public class CurrencySpawnManager : MonoBehaviour, MMEventListener<SpawnCurrencyEvent>
{
    private MMSimpleObjectPooler currencyPool;

    private void Awake()
    {
        currencyPool = GetComponent<MMSimpleObjectPooler>();
    }

    private void OnEnable()
    {
        this.MMEventStartListening<SpawnCurrencyEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<SpawnCurrencyEvent>();
    }

    public void OnMMEvent(SpawnCurrencyEvent eventType)
    {
        SpawnCurrency(eventType.Position);
    }

    public void SpawnCurrency(Vector3 position)
    {
        // Extrae un objeto del pool.
        GameObject currency = currencyPool.GetPooledGameObject();

        if (currency != null)
        {
            // Posiciona el objeto en la ubicación deseada.
            currency.transform.position = position;
            // Activa el objeto.
            currency.SetActive(true);
        }
    }
}