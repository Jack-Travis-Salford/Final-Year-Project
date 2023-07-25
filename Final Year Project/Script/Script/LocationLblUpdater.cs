using TMPro;
using UnityEngine;

public class LocationLblUpdater : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject Player;
    [field: SerializeField] public TextMeshProUGUI Label { private set; get; }

    private void Start()
    {
        Player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    private void Update()
    {
        Label.text = "Player Position: " + Player.transform.position;
    }
}