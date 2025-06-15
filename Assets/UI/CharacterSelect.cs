using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CharacterSelect : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private string characterName;

    private TMP_Text text;
    private Image image;
    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        image = GetComponentInChildren<Image>();

        text.text = characterName;
        image.sprite = sprite;
    }
}
