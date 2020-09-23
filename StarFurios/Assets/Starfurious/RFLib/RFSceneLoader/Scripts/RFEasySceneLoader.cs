using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace RFLib
{

	public class RFEasySceneLoader : MonoBehaviour 
	{

		static RFEasySceneLoader _instance;

		[Tooltip("Name of the scene to load.")]
		public string SceneToLoad;

		[Tooltip("Seconds to display the canvas overlay prior to performing the actual scene load.")]
		public float PreloadDelay = 0.25f;

		[Tooltip("Seconds to wait before auto-activating the loaded scene.")]
		public float ActivateDelay = 0.25f; // Delay once scene has loaded

		[Tooltip("Time to fade in the overlay, in seconds")]
		public float FadeInTime = 0.25f;

		[Tooltip("Time to fade out the overlay, in seconds.")]
		public float FadeOutTime = 0.25f;



		AsyncOperation asynop = null; // Reference to the async load operation
		bool loadingStarted = false;


		// Keep some metrics...
		float loadStartTime = 0;
		float loadStopTime = 0;

		CanvasGroup overlayCanvasGroup;	// Must be a child of this game object
		Canvas		overlayCanvas;		

		/// <summary>
		/// Static Instance reference
		/// </summary>
		/// <value>The instance.</value>
		public static RFEasySceneLoader Instance	{	get{ 	return _instance; 	}}

		/// <summary>
		/// Indicates loading is in progress
		/// </summary>
		public bool isLoading { get { return loadingStarted; } }

		void Awake()
		{
			_instance = this;

			overlayCanvas = gameObject.GetComponentInChildren<Canvas>();
			if( overlayCanvas != null )
			{
				overlayCanvasGroup = overlayCanvas.GetComponentInChildren<CanvasGroup>();
				if( overlayCanvasGroup != null ) overlayCanvasGroup.alpha = 0;
				overlayCanvas.gameObject.SetActive( false );
			}

		}

		// Use this for initialization
		void Start () 
		{
			// Set fade in / out time to 0 if less than 0. Can't have negative fade out!
			if(FadeInTime < 0) 	FadeInTime 	 = 0;
			if(FadeOutTime < 0) FadeOutTime  = 0;

			// Dont destroy this object when the loaded scene is activated
			// Will be destroyed once fade out is complete (immediate if fade out time is 0) &&
			DontDestroyOnLoad(gameObject);
		}
		
		// Update is called once per frame
		void Update () {	}

		/// <summary>
		/// Return the load progress.
		/// Load progress %; -1 if no load is going on
		///	Return 0..1 or -1
		/// </summary>
		public float loadProgress
		{
			get 
			{
				if(asynop != null) 
					return asynop.progress;
				else
					return -1;
			}
		}

		/// <summary>
		/// Return time taken to load the next scene
		/// </summary>
		public float loadTime
		{
			get
			{
				return ( loadStopTime - loadStartTime);
			}
		}


		/// <summary>
		/// LoadScene -- by name (sceneName)
		/// Note: The scene to be loaded must exist in the Build Settings-Scenes In Build panel.
		///       if sceneName is specified, that scene will be loaded.  Otherwise, it is expected
		///		 that SceneToLoad is set.
		/// </summary>
		/// <param name="sceneName">Scene name to load</param>
		public void LoadScene(string sceneName=null)
		{

			if(!string.IsNullOrEmpty(sceneName))
			{
				if(! string.IsNullOrEmpty(SceneToLoad))
				{
					// If the user sets the secene to load programatically, and it was set already,
					// Just let them know.
					Debug.LogWarning("Scene To Load already set. Resetting it in LoadScene.");
				}

				SceneToLoad = sceneName;
			}

			if( string.IsNullOrEmpty(SceneToLoad ))
			{
				Debug.LogWarning("No scene to load.  Set the SceneToLoad value or pass a string into LoadScene");
				return;
			}

			// Already loading!
			if( asynop != null || loadingStarted)				return;

			loadStartTime = Time.time;
			loadingStarted = true;

			if( overlayCanvas != null )
				overlayCanvas.gameObject.SetActive( true );

			StartCoroutine(fadeInOverlay());

		}

		/// <summary>
		/// Coroutine that fades in the overlay canvas.  When done, then calls
		/// the load scene logic
		/// </summary>
		IEnumerator fadeInOverlay()
		{
			// Let the container know we're starting to load..
			if( overlayCanvasGroup != null )
			{
				if( FadeInTime > 0 )
				{

					float fadeRate = 1.0f / FadeInTime;
					float currValue = 0;

					while( currValue <= 1.0f)
					{
						overlayCanvasGroup.alpha = Mathf.Lerp( 0, 1, currValue );
						currValue += Time.deltaTime * fadeRate;
						yield return null;
					}
				}

				overlayCanvasGroup.alpha = 1.0f;

				float delay = 0;
				// 1 second delay before we kick off the scene load; make sure user gets a split second to see the loader
				while(delay < PreloadDelay)
				{
					delay += Time.deltaTime;
					yield return null;
				}
			}

			// Once faded in, load the scene
			StartCoroutine( doLoadScene() );

			yield return null;
		}



		/// <summary>
		/// Coroutine that performs the actual scene load request.
		/// </summary>

		IEnumerator doLoadScene()
		{
			asynop = SceneManager.LoadSceneAsync(SceneToLoad);
			if(asynop != null)
			{
				asynop.allowSceneActivation = false;
				// Watch progress until 90%, the break out of the loop
				while( asynop.progress < 0.9f )
				{
					yield return null;
				}

				loadStopTime = Time.time;
				// Put in a bit of a delay if necessary; 
				while(Time.time < loadStopTime + ActivateDelay)
				{
					yield return null;
				}

				ActivateLoadedScene();
			}

			yield return null;
		}

		/// <summary>
		/// Activate the newly loaded scene, then calls the fadeout and destroy logic
		/// </summary>
		public void ActivateLoadedScene()
		{
			asynop.allowSceneActivation = true;
			asynop = null;

			StartCoroutine(fadeOutAndDestroy());
		}


		/// <summary>
		/// Fade the overlay canvas out over a duration of FadeOutTime.
		/// </summary>
		/// <returns>The out and destroy.</returns>
		IEnumerator fadeOutAndDestroy()
		{
			if( overlayCanvasGroup != null )
			{
				if( FadeOutTime > 0 )
				{
					float fadeRate = 1.0f/FadeOutTime;
					float currValue = 0;

					while(currValue < 1.0f)
					{
						overlayCanvasGroup.alpha = Mathf.Lerp(1, 0, currValue);
						currValue += Time.deltaTime * fadeRate;
						yield return null;
					}
				}
				overlayCanvasGroup.alpha = 0;
			}
			// Destroy this game object after a slight delay.
			Destroy(gameObject, 0.01f);
		}
	}
}

