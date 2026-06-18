using UnityEngine;
using UnityEngine.UI;

public class LiveMatchPlayerTokenView : MonoBehaviour
{
    public Image JerseyImage;
    public Text NumberText;
    public Button Button;

    private LiveMatchPlayerTokenData _token;
    private LiveMatchController _controller;

    public void Initialize(LiveMatchPlayerTokenData token, LiveMatchController controller)
    {
        _token = token;
        _controller = controller;
        if (NumberText != null)
        {
            NumberText.text = token == null ? "" : token.JerseyNumber.ToString();
        }

        if (JerseyImage != null)
        {
            Sprite sprite = token == null || string.IsNullOrEmpty(token.JerseyResourcePath)
                ? null
                : TeamAssetService.LoadSprite(token.JerseyResourcePath);
            JerseyImage.sprite = sprite;
            JerseyImage.color = sprite == null
                ? (token != null && token.IsHomeTeam ? new Color(0.12f, 0.38f, 0.95f) : new Color(0.92f, 0.92f, 0.92f))
                : Color.white;
        }

        if (Button != null)
        {
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (_controller != null)
        {
            _controller.ShowTokenDetails(_token);
        }
    }
}
