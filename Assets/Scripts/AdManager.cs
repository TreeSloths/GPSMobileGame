using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour,IUnityAdsListener
{
   private string playStoreID = "4027030";
   private string appStoreID = "4027031";

   private string interstitialAd = "video";
   private string rewardedVideoAd = "rewardedVideo";

   public bool isTargetPlayStore;
   public bool isTestAd;

   private void Start()
   {
      Advertisement.AddListener(this);
      InitialzeAdvertisement();
      PlayInterstitialAd();
      PlayRewardedVideoAd();
   }

   private void InitialzeAdvertisement()
   {
      if (isTargetPlayStore)
      {
         Advertisement.Initialize(playStoreID,isTestAd);
         return;
      }
      Advertisement.Initialize(appStoreID,isTestAd);
   }

   public void PlayInterstitialAd()
   {
      if (!Advertisement.IsReady(interstitialAd))
      {
         return;
      }
      Advertisement.Show(interstitialAd);
   }

   public void PlayRewardedVideoAd()
   {
      if (!Advertisement.IsReady(rewardedVideoAd))
      {
         return;
      }
      Advertisement.Show(rewardedVideoAd);
   }

   public void OnUnityAdsReady(string placementId)
   {
      
   }

   public void OnUnityAdsDidError(string message)
   {
     
   }

   public void OnUnityAdsDidStart(string placementId)
   {
      
   }

   public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
   {
      switch(showResult)
      {
         case ShowResult.Failed:
              break;
         case ShowResult.Skipped:
              break;
         case ShowResult.Finished:
              if(placementId == rewardedVideoAd){ Debug.Log("Reward The player");}
              if(placementId == interstitialAd){ Debug.Log(("Finished interstitial"));}
              break;

      }
   }
}
