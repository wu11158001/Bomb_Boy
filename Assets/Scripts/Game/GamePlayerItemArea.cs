using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GamePlayerItemArea : MonoBehaviour
{
    [SerializeField] Button GamePlayerItemSwitch_Btn;
    [SerializeField] RectTransform GamePlayerItemAreaNode;
    [SerializeField] GamePlayerItem GamePlayerItemSample;

    [Space(30)]
    [Header("項目開關參數")]
    [SerializeField] float SwitchEffectSpeed;

    private Coroutine _coroutine;
    private bool _isDisplay;

    private void Start()
    {
        // 產生遊戲玩家項目
        for (int i = 0; i < GameDataManager.MaxPlayer; i++)
        {
            int index = i;
            GamePlayerItem gamePlayerItem = Instantiate(GamePlayerItemSample.gameObject, GamePlayerItemAreaNode).GetComponent<GamePlayerItem>();
            gamePlayerItem.gameObject.SetActive(true);
            gamePlayerItem.SetItemIndex(index);
        }
        GamePlayerItemSample.gameObject.SetActive(false);

        _isDisplay = true;

        EventListener();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 遊戲玩家項目顯示開關
        GamePlayerItemSwitch_Btn.onClick.AddListener(() =>
        {
            if (_coroutine != null) return;
            _isDisplay = !_isDisplay;
            ItemSwitchEffect(_isDisplay);
        });
    }

    /// <summary>
    /// 項目開關
    /// </summary>
    /// <param name="isDisplay">顯示/關閉</param>
    /// <returns></returns>
    public void ItemSwitchEffect(bool isDisplay)
    {
        _isDisplay = isDisplay;

        if (_coroutine != null) return;
        _coroutine = StartCoroutine(IItemSwitchEffect(_isDisplay));
    }

    /// <summary>
    /// 項目開關效果
    /// </summary>
    /// <param name="isDisplay">顯示/關閉</param>
    /// <returns></returns>
    private IEnumerator IItemSwitchEffect(bool isDisplay)
    {
        if (isDisplay)
        {
            while (GamePlayerItemAreaNode.anchoredPosition.x > 0)
            {
                GamePlayerItemAreaNode.Translate(new Vector3(-SwitchEffectSpeed * Time.deltaTime, 0, 0), Space.Self);
                yield return null;
            }

            GamePlayerItemAreaNode.anchoredPosition = Vector2.zero;
        }
        else
        {
            while (GamePlayerItemAreaNode.anchoredPosition.x > -50)
            {
                GamePlayerItemAreaNode.Translate(new Vector3(-SwitchEffectSpeed * 0.5f * Time.deltaTime, 0, 0), Space.Self);
                yield return null;
            }

            while (GamePlayerItemAreaNode.anchoredPosition.x < GamePlayerItemAreaNode.sizeDelta.x)
            {
                GamePlayerItemAreaNode.Translate(new Vector3(SwitchEffectSpeed * Time.deltaTime, 0, 0), Space.Self);
                yield return null;
            }

            GamePlayerItemAreaNode.anchoredPosition = new Vector2(GamePlayerItemAreaNode.sizeDelta.x, 0);
        }

        _coroutine = null;
    }
}
