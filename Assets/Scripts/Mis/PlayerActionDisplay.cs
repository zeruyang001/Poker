using UnityEngine;
using UnityEngine.UI;

public class PlayerActionDisplay : MonoBehaviour
{
    [SerializeField] private Image actionImage;
    [SerializeField] private Sprite[] actionSprites; // ��Inspector�����ã�˳���Ӧö��

    public void SetState(PlayerActionState state)
    {
        if (state == PlayerActionState.None)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        actionImage.sprite = actionSprites[(int)state - 1]; // -1 ��ΪNone��0
    }
}