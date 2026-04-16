using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    public static CustomerSpawner Instance;

    [Header("Prefabs")]
    public GameObject[] customerPrefabs; // fallback if card has no prefab assigned

    [Header("Spots")]
    public Transform spawnPoint;
    public Transform registerSpot;
    public Transform[] waitingSpots;

    private List<Customer> activeCustomers = new List<Customer>();
    private bool[] spotOccupied;

    void Awake()
    {
        Instance = this;
        spotOccupied = new bool[waitingSpots.Length];
    }

    public void SpawnCustomers(List<CustomerCard> queue)
    {
        StartCoroutine(SpawnSequence(queue));
    }

    IEnumerator SpawnSequence(List<CustomerCard> queue)
    {
        for (int i = 0; i < queue.Count; i++)
        {
            SpawnCustomer(queue[i]);
            yield return new WaitForSeconds(0.8f);
        }
    }

    void SpawnCustomer(CustomerCard card)
    {
        // use card's prefab, fallback to random if not assigned
        GameObject prefab = card.customerPrefab != null
            ? card.customerPrefab
            : customerPrefabs[Random.Range(0, customerPrefabs.Length)];

        GameObject obj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        Customer customer = obj.GetComponent<Customer>();
        if (customer == null)
            customer = obj.AddComponent<Customer>();

        int spotIndex = GetFreeSpot();
        if (spotIndex == -1)
        {
            Debug.Log("No free waiting spots!");
            return;
        }

        spotOccupied[spotIndex] = true;
        customer.Setup(card, spotIndex);
        activeCustomers.Add(customer);
        customer.MoveTo(waitingSpots[spotIndex].position);
    }

    int GetFreeSpot()
    {
        for (int i = 0; i < spotOccupied.Length; i++)
            if (!spotOccupied[i]) return i;
        return -1;
    }

    public void MoveCustomerToRegister(CustomerCard card)
    {
        Customer customer = GetCustomerByCard(card);
        if (customer == null) return;
        customer.MoveTo(registerSpot.position);
    }

    public void DespawnCustomer(CustomerCard card)
    {
        Customer customer = GetCustomerByCard(card);
        if (customer == null) return;

        spotOccupied[customer.WaitingSpotIndex] = false;
        activeCustomers.Remove(customer);

        customer.MoveTo(spawnPoint.position, () =>
        {
            Destroy(customer.gameObject);
        });
    }


    public void DespawnAll()
    {
        foreach (Customer c in activeCustomers)
            if (c != null) Destroy(c.gameObject);

        activeCustomers.Clear();
        for (int i = 0; i < spotOccupied.Length; i++)
            spotOccupied[i] = false;
    }

    Customer GetCustomerByCard(CustomerCard card)
    {
        foreach (Customer c in activeCustomers)
            if (c.AssignedCard == card) return c;
        return null;
    }
}