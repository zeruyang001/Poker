using UnityEngine;
using UnityEngine.UI;

public class PlayerActionDisplay : MonoBehaviour
{
    [SerializeField] private Image actionImage;
    [SerializeField] private Sprite[] actionSprites; // 在Inspector中设置，顺序对应枚举

    public void SetState(PlayerActionState state)
    {
        if (state == PlayerActionState.None)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        actionImage.sprite = actionSprites[(int)state - 1]; // -1 因为None是0
    }
}