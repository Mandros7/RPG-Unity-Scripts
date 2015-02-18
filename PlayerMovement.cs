using UnityEngine;
using System.Collections;

/*
Este script se encarga del movimiento del jugador y de la rotacion natural al cambiar de dirección
Tiene que aplicarse a un objeto que tenga un componente RigidBody. 
*/

public class PlayerMovement : MonoBehaviour
{
	// Declaracion de variables publicas (para modificarlas en el testeo)
	public float turnSmoothing = 15f;   // Un valor que indica como de suave sera el giro del jugador
	public float speed = 5.0f;
	
	void FixedUpdate ()
	{
		// Obtenemos las entradas y las almacenamos en variables
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		
		MovementManagement(h, v);
	}
	
	
	void MovementManagement (float horizontal, float vertical)
	{
		// Primero comprobamos que hay inputs.
		if(horizontal != 0f || vertical != 0f)
		{
			// Le pasamos los valores a la funcion de rotacion
			Rotating(horizontal, vertical);

			//Comprobamos si estamos moviendonos en diagonal. Pulsando mas de una tecla.
			if(horizontal != 0f && vertical != 0f){
				/* La combinar las coordenadas horizontal y vertical de esta forma (la componente Y es altura)
					Se consigue cambiar la orientación del movimiento, de forma que el personaje ahora se mueve 
					hacia arriba al pulsar la tecla Up o W.
				*/
				Vector3 targetDirection = new Vector3(horizontal+vertical, 0f, vertical-horizontal);
				rigidbody.MovePosition(rigidbody.position + targetDirection * speed * Time.deltaTime);
			}
			else {
				// Para movimientos con una sola componente
				Vector3 targetDirection = new Vector3(horizontal+vertical, 0f, vertical-horizontal);
				if (vertical!=0f){
					// En el caso de movimiento vertical [arriba y abajo], es necesario ajustar la velocidad (x1.5)
					rigidbody.MovePosition(rigidbody.position + targetDirection * speed * Time.deltaTime * 1.5f);
				}
				else {
					rigidbody.MovePosition(rigidbody.position + targetDirection * speed * Time.deltaTime);
				}
			}
		}
	}
	
	
	void Rotating (float horizontal, float vertical)
	{
		// Nuevo vector con la combinación de componentes (la suma y resta cambian la orientacion)
		Vector3 targetDirection = new Vector3(horizontal+vertical, 0f, vertical-horizontal);
	
		Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
		
		Quaternion newRotation = Quaternion.Lerp(rigidbody.rotation, targetRotation, turnSmoothing * Time.deltaTime);
		
		rigidbody.MoveRotation(newRotation);
	}

}