using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public enum HTTP_METHOD			{ GET, POST } 

public enum HTTP_CACHE_POLICY   { NONE,  IMAGE  } 

public enum QUERY_PARAM_TYPE	{ VALUE, STRING } 

public enum QUERY_RESULT		{ NONE, SUCCESS, CANCELED, ERROR, TIMEOUT } 

//********************************************************************************************************
//
//********************************************************************************************************

static public class Guid
{
	static public long @new
	{
		get
		{
			return System.Guid.NewGuid().GetHashCode();
		}
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public interface IProgressIndicator
{
	string text { get; set; }

	float  prc  { get; set; }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class HTTPReqDelegate
{
	public delegate void Delegate( HTTPReq req, params object[] paramsList );


	private Delegate m_delegate = null;

	public  Delegate delegates { get { return m_delegate; } }

	public 	object[] @params   { get; set; }


	public      HTTPReqDelegate( Delegate dlg, params object[] paramsList ) { m_delegate = dlg; @params = paramsList; }

	public void Set            ( Delegate dlg ) { m_delegate  = dlg; }

	public void Add            ( Delegate dlg ) { m_delegate += dlg; }

	public void Invoke         ( HTTPReq  req ) { if( m_delegate != null ) m_delegate( req, @params ); }
}

//********************************************************************************************************
//
//********************************************************************************************************

public class HTTPReq
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	#if UNITY_EDITOR

		static public float  delaySimulation         { get; set; }

			   private float delaySimulationStarted  { get; set; }

			   public  bool  delaySimulationEllapsed { get { return ( onCompletion == null ) || ( delaySimulation <= 0.0f ) || ( ( Time.time - started ) > delaySimulation ); } }
	#else

		static public float  delaySimulation         { get { return 0.0f; } set {} }

			   private float delaySimulationStarted  { get { return 0.0f; } set {} }

			   public  bool  delaySimulationEllapsed { get { return true; } }

	#endif

	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private TextureCache m_cache = new TextureCache();

	static public  TextureCache cache { get { return m_cache; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	       public const float TIMEOUT_MIN     = 5.0f;

	       public const float TIMEOUT_MAX     = 30.0f;

	       public const float TIMEOUT_DEFAULT = TIMEOUT_MIN;

	static public IProgressIndicator defaultProgressIndicator { get; set; }

	static public IProgressIndicator defaultTimeoutIndicator  { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private HTTP_METHOD	m_method  = HTTP_METHOD.POST;

	private string		m_baseURL = string.Empty;

	private string		m_URI     = string.Empty;

	private string		m_URL     = string.Empty;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public HTTPReq( string paramBaseURL, HTTP_METHOD mthd, HTTP_CACHE_POLICY policy = HTTP_CACHE_POLICY.NONE, Texture paramTextureNotFound = null )
	{
		method            = mthd;

		baseURL           = paramBaseURL;

		URI               = string.Empty;

		parameters        = new List< KeyValuePair< string, string > >();

		form              = new WWWForm();

		started           = 0.0f;

		timeout           = TIMEOUT_DEFAULT;

		result            = QUERY_RESULT.NONE;

		texture           = null;

		response          = string.Empty;

		httpErrorCode     = -1;

		textureNotFound   = paramTextureNotFound;

		node              = new LinkedListNode< HTTPReq >( this );

		progressIndicator = defaultProgressIndicator;

		timeoutIndicator  = defaultTimeoutIndicator;

		canceled          = false;

		onCompletion      = null;

		cachePolicy       = policy;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public HTTP_METHOD								method            { get { return m_method;  } set { m_method  = value; dirty = true; } }

	public string									baseURL           { get { return m_baseURL; } set { m_baseURL = value; dirty = true; } }

	public string									URI               { get { return m_URI;     } set { m_URI     = value; dirty = true; } }

	public string									URL               { get { if( dirty ) { UpdateURL(); } return m_URL; } }

	public List< KeyValuePair< string, string > >	parameters        { get; set; }

	public WWWForm									form              { get; set; }

	public float									started           { get; set; }

	public float									timeout           { get; set; }

	public bool										didTimeout        { get { return ( timeout > 0.0f ) && ( ( Time.time - started ) > timeout ); } }

	public QUERY_RESULT								result            { get; set; }

	public Texture                                  texture           { get; set; }

	public string                                   response          { get; set; }

	public int                                      httpErrorCode     { get; set; }

	public Texture                                  textureNotFound   { get; set; }

	public LinkedListNode< HTTPReq >				node              { get; set; }

	public IProgressIndicator                       progressIndicator { get; set; }

	public IProgressIndicator                       timeoutIndicator  { get; set; }

	public bool                                     dirty             { get; set; }

	public bool                                     canceled          { get; set; }

	public HTTPReqDelegate                          onCompletion      { get; set; }

	public HTTP_CACHE_POLICY						cachePolicy       { get; set; }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void AddParameter( string key, string value, QUERY_PARAM_TYPE paramType )
	{
		if( string.IsNullOrEmpty( key   ) ) return;

		if( string.IsNullOrEmpty( value ) ) return;

		if( method == HTTP_METHOD.GET )
		{
			parameters.Add( new KeyValuePair< string, string >( key, ( ( paramType == QUERY_PARAM_TYPE.VALUE ) ? value : WWW.EscapeURL( value ) ) ) );
		}
		else
		{
			form.AddField( key, value );
		}

		dirty = true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void UpdateURL()
	{
		httpErrorCode = 200;

		texture       = null;

		response      = string.Empty;

		m_URI         = string.Empty;

		if( method == HTTP_METHOD.GET )
		{
			if( parameters.Count > 0 ) m_URI += "?";

			for( int p = 0; p < parameters.Count; ++p )
			{
				KeyValuePair< string, string > param = parameters[ p ];

				m_URI += "&" + param.Key + "=" + param.Value;
			}
		}

		m_URL = CORE.ENCODE.To< System.Text.UTF8Encoding >( m_baseURL + m_URI );

		dirty = false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public int GetHttpErrorCode( Dictionary< string, string > responseHeaders )
	{
		string httpStatus = string.Empty;

		int    httpError  = -1;

		if( responseHeaders.TryGetValue( "STATUS", out httpStatus ) )
		{
			string[] status = httpStatus.Split( ' ' );

			if( status.Length > 1 ) int.TryParse( status[ 1 ], out httpError );
		}

		return httpError;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public IEnumerator Submit()
	{
		if( timeoutIndicator != null ) { if( timeout > 0.0f ) { timeoutIndicator.prc = 0.0f; } }

		if( ( method == HTTP_METHOD.GET ) && ( cachePolicy == HTTP_CACHE_POLICY.IMAGE ) )
		{
			if( ( texture = cache.Get( URL ) ) != null )
			{
				CORE.UTILS.Log( "USING CACHE FOR " + URL );

				SubmitEnd( null, QUERY_RESULT.SUCCESS );

				yield break;
			}
		}


		CORE.UTILS.Log( URL );

		WWW req = ( method == HTTP_METHOD.GET ) ? new WWW( URL ) : new WWW( URL, form );

		result  = QUERY_RESULT.NONE;

		started = Time.time;

		delaySimulationStarted = Time.time;

		while( ( req.isDone == false ) || ( delaySimulationEllapsed == false ) )
		{
			if( didTimeout ) { response = "TIMEOUT";  SubmitEnd( req, QUERY_RESULT.TIMEOUT );  yield break; }

			if( canceled   ) { response = "CANCELED"; SubmitEnd( req, QUERY_RESULT.CANCELED ); yield break; }

			if( progressIndicator != null ) { progressIndicator.prc = req.progress; }

			if( timeoutIndicator  != null ) { if( timeout > 0.0f ) { timeoutIndicator.prc = ( Time.time - started ) / timeout; } }

			yield return req;
		}


		if( req.error != null ) { response = req.error; SubmitEnd( req, QUERY_RESULT.ERROR   ); }

		else                    { response = req.text;  SubmitEnd( req, QUERY_RESULT.SUCCESS ); }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void SubmitEnd( WWW req, QUERY_RESULT paramResult )
	{
		result = paramResult;

		if( req != null )
		{
			httpErrorCode  = GetHttpErrorCode( req.responseHeaders );

			bool httpError = ( httpErrorCode >= 0 ) && ( httpErrorCode != 200 );

			if ( httpError ) CORE.UTILS.Log( response );

			if( cachePolicy == HTTP_CACHE_POLICY.IMAGE )
			{
				texture = httpError ? textureNotFound : req.texture;

				if( ( texture != null ) && ( texture != textureNotFound ) ) cache.Add( URL, texture );
			}
		}


		if( progressIndicator != null ) { progressIndicator.prc = 1.0f; }

		if( onCompletion != null )
		{
			onCompletion.Invoke( this );

			onCompletion = null;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public QUERY_RESULT SubmitImmediate()
	{
		if( node.List != null ) return QUERY_RESULT.ERROR;

		result = QUERY_RESULT.NONE;

		IEnumerator e = Submit();

		while( result == QUERY_RESULT.NONE ) { e.MoveNext(); }

		return result;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool SubmitAsync( HTTPReqDelegate handler = null )
	{
		if( node.List != null ) return false;

		if( HTTPReqs.Instance != null )
		{
			onCompletion = handler;

			return HTTPReqs.Instance.Queue( this );
		}

		return false;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Cancel()
	{
		if( result == QUERY_RESULT.NONE )
		{
			if( canceled == false )
			{
				canceled = true;

				if( node.List != null ) node.List.Remove( node );
			}
		}
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class HTTPReqs : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	static private HTTPReqs m_instance = null;

	static public  HTTPReqs Instance { get { return m_instance; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	[ SerializeField ]	public bool                   m_logOnly = false;

						private LinkedList< HTTPReq > m_queries = new LinkedList< HTTPReq >();

						private HTTPReq               m_query   = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool busy
	{
		get
		{
			if( m_query != null )     return true;

			if( m_queries.Count > 0 ) return true;

			return false;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Queue( HTTPReq query )
	{
		if( query           == null ) return false;

		if( query.node.List != null ) return false;

		m_queries.AddLast( query.node );

		query.response = string.Empty;

		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private IEnumerator SubmitQuery()
	{
		if( m_query   == null  ) yield break;

		if( m_logOnly == false )
		{
			m_query.result = QUERY_RESULT.NONE;

			while( m_query.result == QUERY_RESULT.NONE )
			{
				yield return m_query.Submit();
			}
		}

		if( m_query.node.List != null )
		{
			m_query.node.List.Remove( m_query.node );
		}

		m_query = null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Awake()
	{
		if( m_instance == null ) m_instance = this;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnDestroy()
	{
		if( m_instance == this ) m_instance = null;

		Cancel();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Cancel()
	{
		StopAllCoroutines();

		m_queries.Clear();

		if( m_query != null ) { m_query.Cancel(); m_query = null; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		if( ( m_query == null ) && ( m_queries.Count > 0 ) )
		{
			m_query = m_queries.First.Value;

			if( m_query != null )
			{
				StartCoroutine( SubmitQuery() );
			}
		}
	}
}