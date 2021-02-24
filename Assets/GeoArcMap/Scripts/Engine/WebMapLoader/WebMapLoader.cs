using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CORE;

//********************************************************************************************************
//
//********************************************************************************************************

public enum WEB_MAP_TYPE { RoadMap, Satellite, Terrain, Hybrid }

//********************************************************************************************************
//
//********************************************************************************************************

public class WebMapParams
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	public  WEB_MAP_TYPE m_TYPE      = WEB_MAP_TYPE.Satellite;

	public  int			 m_size      = 512;

	public  bool		 m_hres      = false;

	public  Vector3      m_coordGeo  = Vector3.zero;

	public  GridCoords   m_coordGrid = new GridCoords( int.MaxValue, int.MaxValue );

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WEB_MAP_TYPE type       { get { return m_TYPE;      } set { m_TYPE      = value; } }

	public int          size       { get { return m_size;      } set { m_size      = value; } }

	public bool         hres       { get { return m_hres;      } set { m_hres      = value; } }

	public Vector3      coordGeo   { get { return m_coordGeo;  } set { m_coordGeo  = value; } }

	public GridCoords   coordGrid  { get { return m_coordGrid; } set { m_coordGrid = value; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Equals( WebMapParams otherParams )
	{
		if( m_coordGrid != otherParams.m_coordGrid ) return false;

		if( m_coordGeo  != otherParams.m_coordGeo  ) return false;

		if( m_TYPE      != otherParams.m_TYPE      ) return false;

		if( m_size      != otherParams.m_size      ) return false;

		if( m_hres      != otherParams.m_hres      ) return false;

		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Set( WebMapParams otherParams )
	{
		m_TYPE      = otherParams.m_TYPE;

		m_size      = otherParams.m_size;

		m_hres      = otherParams.m_hres;

		m_coordGeo  = otherParams.m_coordGeo;

		m_coordGrid = otherParams.m_coordGrid;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Reset()
	{
		m_TYPE      = WEB_MAP_TYPE.Satellite;

		m_size      = 512;

		m_hres      = false;

		m_coordGeo  = Vector3.zero;

		m_coordGrid = new GridCoords( int.MaxValue, int.MaxValue );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public override string ToString()
	{
		return string.Format( "zoom:{0} lat:{1} lng:{2} hres:{3} size:{4} type:{5} tileX:{6} tileY:{7}",
			
			                  m_coordGeo.z, m_coordGeo.y, m_coordGeo.x, m_hres, m_size, m_TYPE.ToString(), m_coordGrid.X, m_coordGrid.Y );
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class WebMapRequest
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

    public enum STATUS      { NEW, PROCESSING, CANCELED, COMPLETED      } 

    public enum DROP_REASON { NEW, CANCELED, COMPLETED, CONTEXT_CHANGED }

	//****************************************************************************************************
	//
	//****************************************************************************************************

    private STATUS   						m_status  = STATUS.NEW;

	private WebMapParams					m_params  = new WebMapParams();

	private Texture							m_texture = null;

	private LinkedListNode< WebMapRequest > m_node    = null;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WebMapRequest()
	{
		m_node = new LinkedListNode< WebMapRequest >( this );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

    public STATUS  status       { get { return m_status;                    } set { m_status = value;             } }
	
	public bool    @new         { get { return status == STATUS.NEW;        } set { status = STATUS.NEW;          } }
	
	public bool    processing   { get { return status == STATUS.PROCESSING; } set { status = STATUS.PROCESSING;   } }
	
	public bool    canceled     { get { return status == STATUS.CANCELED;   } set { status = STATUS.CANCELED;     } }
	
	public bool    completed    { get { return status == STATUS.COMPLETED;  } set { status = STATUS.COMPLETED;    } }

	public Texture texture      { get { return m_texture;                   } set { m_texture = value;            } }

	public bool    queued       { get { return m_node.List != null;         }  }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public LinkedListNode< WebMapRequest > node
	{
		get { return m_node; }
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void Abort()
	{
		if( m_node.List != null )
		{
			m_node.List.Remove( m_node );
		}

		if( m_status != STATUS.NEW )
		{
			m_params.Reset();

			m_status = STATUS.CANCELED;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WebMapParams @params
	{
		get { return m_params; }

		set
		{
			if( m_params.Equals( value ) == false )
			{
				Abort();

				m_params.Set( value );

				m_status = STATUS.NEW;
			}
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Cancel()
	{
		if( ( m_node.List != null ) || ( processing ) )
		{
			Abort();
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Invalidate()
	{
		Abort();

		m_params.Reset();

		texture  =  null;
	}
}

//********************************************************************************************************
//
//********************************************************************************************************

public class WebMapLoader : MonoBehaviour
{
	//****************************************************************************************************
	//
	//****************************************************************************************************

	private WebMap                      m_map       = null;

	private TextureCache				m_cache     = new TextureCache();

	private LinkedList< WebMapRequest >	m_queue     = new LinkedList< WebMapRequest >();

	private WebMapRequest				m_request   = null;

	private string						m_API_KEY   = string.Empty;

	private IProgressIndicator          m_indicator = null;

	private int							m_workLoad  = 0;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	[ SerializeField ] public bool m_logOnly       = false;

	[ SerializeField ] public bool m_cacheDisabled = false;

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public WebMap             map       { get { return m_map;       } set { m_map       = value; } }

	public string             API_KEY   { get { return m_API_KEY;   } set { m_API_KEY   = value; } }

	public bool               logOnly   { get { return m_logOnly;   } set { m_logOnly   = value; } }

	public IProgressIndicator indicator { get { return m_indicator; } set { m_indicator = value; } }

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public bool Queue( WebMapRequest req )
	{
		if( req           == null  ) return false;

		if( req.@new      == false ) return false;

		if( req.node.List != null  ) return false;

		m_queue.AddLast( req.node );

		m_workLoad = m_queue.Count;

		return true;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private WebMapRequest DeQueue()
	{
		if( m_queue.Count > 0 )
		{
			WebMapRequest req = m_queue.First.Value;

			m_queue.RemoveFirst();

			return req;
		}

		return null;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void DropCurRequest( WebMapRequest.DROP_REASON reason )
	{
		if( m_request != null )
		{
			switch( reason )
			{
				case WebMapRequest.DROP_REASON.NEW             :                             break;

				case WebMapRequest.DROP_REASON.CANCELED        : m_request.canceled  = true; break;

				case WebMapRequest.DROP_REASON.COMPLETED       : m_request.completed = true; break;

				case WebMapRequest.DROP_REASON.CONTEXT_CHANGED :                             break;
			}
		}

		m_request = null;

		UpdateIndicator( false );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private string GetGoogleMapRequestURL( WebMapParams @params )
	{
		var url       = "https://maps.googleapis.com/maps/api/staticmap";

		var urlParams = "";

		urlParams += "center="   + WWW.UnEscapeURL( string.Format ("{0},{1}", @params.coordGeo.y, @params.coordGeo.x ) );
		
		urlParams += "&mobile="  + m_map.mobile.ToString();

		urlParams += "&zoom="    + @params.coordGeo.z.ToString();

		urlParams += "&size="    + WWW.UnEscapeURL( string.Format( "{0}x{1}", @params.m_size, @params.m_size ) );

		urlParams += "&scale="   + ( @params.m_hres ? "2" : "1" );

		urlParams += "&maptype=" + @params.m_TYPE.ToString().ToLower();

		urlParams += "&sensor="  + "false";

		urlParams += "&key="     + m_API_KEY;

		return url + "?" + urlParams;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private string GetESRIMapRequestURL( WebMapParams @params )
	{
		var     url       = "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile";

		var     urlParams = ( @params.coordGeo.z ).ToString() + "/" + @params.coordGrid.Y.ToString() + "/" + @params.coordGrid.X.ToString();

		return  url + "/" + urlParams;
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private IEnumerator RequestMap()
	{
		if( m_request != null )
		{
			string url = string.Empty;

			switch( m_map.source )
			{
				case WebMap.SOURCE.GOOGLE: url = GetGoogleMapRequestURL( m_request.@params ); break;

				case WebMap.SOURCE.ESRI  : url = GetESRIMapRequestURL  ( m_request.@params ); break;
			}

			Texture texture = ( m_cacheDisabled == false ) ? m_cache.Get( url ) : null;

			if( texture != null )
			{
				CORE.UTILS.Log( "USING CACHE FOR " + url );

				if( object.ReferenceEquals( m_request.texture, texture ) == false )
				{
					m_request.texture = texture;
				}

				DropCurRequest( WebMapRequest.DROP_REASON.COMPLETED );

				yield break;
			}



			CORE.UTILS.Log( "LOADING > " + m_map.source.ToString() + " > " + url );

			WWW HTTPSReq = ( m_logOnly == false ) ? new WWW ( url ) : null;

			while( ( HTTPSReq != null ) && ( HTTPSReq.isDone == false ) )
			{
				if( m_request.canceled || m_request.@new )
				{
					DropCurRequest( WebMapRequest.DROP_REASON.CONTEXT_CHANGED );

					yield break;
				}

				yield return HTTPSReq;
			}


			if( ( HTTPSReq != null ) && ( HTTPSReq.error != null ) )
			{
				DropCurRequest( WebMapRequest.DROP_REASON.CANCELED );

				yield break;
			}


			texture = ( HTTPSReq != null ) ? HTTPSReq.texture : null;

			if( texture != null )
			{
				CORE.UI.ClampTexture( texture, FilterMode.Bilinear );

				texture.name = url;

				m_cache.Add( url, texture );

				m_request.texture = texture;
			}

			DropCurRequest( WebMapRequest.DROP_REASON.COMPLETED );
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void FlushCache()
	{
		m_cache.Flush();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Cancel()
	{
		StopAllCoroutines();

		m_queue.Clear();

		DropCurRequest( WebMapRequest.DROP_REASON.CANCELED );
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void OnDestroy()
	{
		Cancel();

		m_cache.Flush();
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	private void UpdateIndicator( bool accountForCurRequest )
	{
		if( m_indicator != null )
		{
			m_indicator.text = string.Format( "{0} Tiles remaining", m_queue.Count + ( accountForCurRequest ? 1 : 0 ) );

			m_indicator.prc  = ( m_workLoad > 0 ) ? ( 1.0f - ( ( float )m_queue.Count / ( float )m_workLoad ) ) : 1.0f;
		}
	}

	//****************************************************************************************************
	//
	//****************************************************************************************************

	public void Update()
	{
		if( m_map == null )
		{
			return;
		}

		if( m_request == null )
		{
			while( ( m_request = DeQueue() ) != null )
			{
				if( m_request.canceled )
				{
					m_request = null;

					UpdateIndicator( false );

					continue;
				}

				UpdateIndicator( true );

				m_request.processing = true;

				StartCoroutine( RequestMap() );

				break;
			}
		}
	}
}
