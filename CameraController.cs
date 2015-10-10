using UnityEngine;
using System.Collections;

//Versión 2 del movimiento de la cámara. Se define un rango fuera del cual la camara seguirá al target

public class SmoothFollow2 : MonoBehaviour {



	public Transform target; //Objetivo. En este caso el jugador
	public float distance;	
	//public float height = 3.0f;
	public float damping = 5.0f;
	public float rotationDamping = 10.0f;
	private Vector3 offset; // Mide la posicion inicial relativa de ambos objetos
	private float current_distance; // Distancia entre ambos objetos
	public float range; // Rango máximo de distancia permitida sin mover la camara
	private Vector3 old_wantedPosition;
	public float fixed_Horizontal = 2.0f; //Variable de ajuste del seguimiento horizontal

	void Start () {
		// Inicialmente se asignan valores a las variables.
		offset = transform.position - target.transform.position;
		distance = Vector3.Distance (transform.position, target.transform.position);
		old_wantedPosition = transform.position;
	}

	bool checkRange(){
		//Funcion que comprueba si se ha salido del rango
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		current_distance = Vector3.Distance (transform.position, target.transform.position);
		if (v==0 && h!=0){
			 //Se ajusta el valor cuando el movimiento es solo horizontal
			current_distance*=fixed_Horizontal;
		}
		if (Mathf.Abs (current_distance-distance)>range){
			return true;
		}
		else {
			return false;
		}
	}

	void FixedUpdate () {
		Vector3 wantedPosition;
		// Si estamos fuera del rango actualizamos la posicion deseada de la camara.
		if (checkRange ()) {
						wantedPosition = new Vector3 (target.transform.position.x + offset.x, target.transform.position.y + offset.y, target.transform.position.z + offset.z);
						old_wantedPosition = wantedPosition;
				}
		// Movemos la camara.
		transform.position = Vector3.Slerp (transform.position, old_wantedPosition, Time.deltaTime * damping);
	}
}