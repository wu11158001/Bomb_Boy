using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class GamePlayerItemArea : MonoBehaviour
{
    [SerializeField] Button GamePlayerItemSwitch_Btn;
    [SerializeField] RectTransform GamePlayerItemAreaNode;
    [SerializeField] GamePlayerItem GamePlayerItemSample;
    [SerializeField] RectTransform MoveAreaNode;

    [Space(30)]
    [Header("項目開關參數")]
    [SerializeField] float SwitchEffectSpeed;

    private Coroutine _coroutine;
    private bool _isDisplay;

    private GamePlayerItem[] gamePlayerItems = new GamePlayerItem[4];

    // 背景隨項目數量增加高度
    private float AreaAddHeight;

    private void Start()
    {
        GridLayoutGroup gridLayoutGroup = GamePlayerItemAreaNode.GetComponent<GridLayoutGroup>();
        AreaAddHeight = gridLayoutGroup.spacing.y + gridLayoutGroup.cellSize.y;

        // 產生遊戲玩家項目
        for (int i = 0; i < GameDataManager.MaxPlayer; i++)
        {
            int index = i;
            GamePlayerItem gamePlayerItem = Instantiate(GamePlayerItemSample.gameObject, GamePlayerItemAreaNode).GetComponent<GamePlayerItem>();
            gamePlayerItem.gameObject.SetActive(true);
            gamePlayerItem.SetItemIndex(index);
            gamePlayerItems[i] = gamePlayerItem;
        }
        GamePlayerItemSample.gameObject.SetActive(false);

        _isDisplay = true;

        EventListener();
    }

    private void Update()
    {
        int childActiveCount = gamePlayerItems.Count(x => x.GetNodeObjActive());
        GamePlayerItemAreaNode.sizeDelta = new Vector2(
            GamePlayerItemAreaNode.sizeDelta.x, 
            AreaAddHeight * childActiveCount);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 遊戲玩家項目顯示開關
        GamePlayerItemSwitch_Btn.onClick.AddListener(() =>
        {
            AudioManager.I.PlaySound(SoundEnum.Click);

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
            while (MoveAreaNode.anchoredPosition.x > 0)
            {
                MoveAreaNode.Translate(new Vector3(-SwitchEffectSpeed * Time.deltaTime, 0, 0), Space.Self);
                yield return null;
            }

            MoveAreaNode.anchoredPosition = Vector2.zero;
        }
        else
        {
            while (MoveAreaNode.anchoredPosition.x > -50)
            {
                MoveAreaNode.Translate(new Vector3(-SwitchEffectSpeed * 0.5f * Time.deltaTime, 0, 0), Space.Self);
                yield return null;
            }

            while (MoveAreaNode.anchoredPosition.x < MoveAreaNode.sizeDelta.x)
            {
                MoveAreaNode.Translate(new Vector3(SwitchEffectSpeed * Time.deltaTime, 0, 0), Space.Self);
                yield return null;
            }

            MoveAreaNode.anchoredPosition = new Vector2(MoveAreaNode.sizeDelta.x, 0);
        }

        _coroutine = null;
    }
}
