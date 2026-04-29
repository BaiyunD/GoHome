using UnityEngine;
using UnityEngine.UI;

public sealed class ShopCommoditySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedStateRoot;

    private ShopCommodityViewModel _viewModel;
    private ShopPagePresenter _presenter;

    public void Bind(ShopCommodityViewModel viewModel, ShopPagePresenter presenter)
    {
        _presenter = presenter;
        _viewModel = viewModel;
        SetSelected(false);
        if (iconImage != null)
        {
            iconImage.sprite = viewModel.Icon;
            iconImage.enabled = viewModel.Icon != null;
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedStateRoot != null)
        {
            selectedStateRoot.SetActive(selected);
        }
    }

    public void OnClick()
    {
        if (_presenter == null)
        {
            return;
        }

        _presenter.SelectCommodity(_viewModel.CommodityId);
    }
}
