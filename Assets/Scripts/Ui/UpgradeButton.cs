using Gameplay;
using Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    [RequireComponent(typeof(Button))]
    public abstract class UpgradeButton : MonoBehaviour
    {
        [SerializeField] private PlayerMoneyBalance _balance;
        [SerializeField] private Button _button;
        [SerializeField] private Text _levelPresenterText;
        [SerializeField] private Text _pricePresenterText;
        [SerializeField] private uint _currentLevel = 1;
        [SerializeField] private uint _price = 100;
        [SerializeField] private float _priceMultiplier = 1.5f;
        
        protected string UpgradeName;
        protected uint CurrentLevel => _currentLevel;

        private void Awake()
        {
            UpdatePricePresenter();
            SetUpgradeName();

            _price = (uint) PlayerPrefs.GetInt($"{UpgradeName}price", 100);
            
            if (SaveProgress.HasSave(UpgradeName))
            {
                LoadSave();
            }
        }

        private void Reset()
        {
            _balance = FindObjectOfType<PlayerMoneyBalance>();
            _button = GetComponent<Button>();
            _levelPresenterText = GetComponentInChildren<Text>();
        }

        private void OnEnable()
        {
            _balance.AmountChanged += CheckBuyAbility;
            _button.onClick.AddListener(Buy);
            CheckBuyAbility(_balance.Amount);
            UpdateLevelPresenter();
        }

        private void FixedUpdate()
        {
            if (_balance.Amount < _price)
            {
                _button.interactable = false;
            }
        }

        protected void LoadSave()
        {
            _currentLevel = (uint) PlayerPrefs.GetInt(UpgradeName);
        }

        protected abstract void Upgrade();

        protected virtual void SetUpgradeName(){}

        private void Buy()
        {
            if (!_balance.TrySpend(_price)) return;

            _currentLevel++;
            UpdateLevelPresenter();
            IncreasePrice();
            Upgrade();
            PlayerPrefs.SetInt(UpgradeName, (int) _currentLevel);
        }

        private void CheckBuyAbility(uint amount)
        {
            _button.interactable = _price <= amount;
        }

        private void IncreasePrice()
        {
            _price = (uint) (_price * _priceMultiplier);
            UpdatePricePresenter();
            PlayerPrefs.SetInt($"{UpgradeName}price", (int) _price);
        }

        private void UpdatePricePresenter()
        {
            _pricePresenterText.text = $"{_price}$";
        }

        private void UpdateLevelPresenter()
        {
            _levelPresenterText.text = _currentLevel.ToString();
        }
    }
}