namespace Core.Utils
{
    using UnityEngine;

    public class Spring
    {
        // Variables internas (privadas)
        private float _strength;
        private float _damper;
        private float _target;
        private float _velocity;
        private float _value = 0f;

        // Propiedades Públicas con Getters y Setters
        public float Strength
        {
            get => _strength;
            set => _strength = value;
        }

        public float Damper
        {
            get => _damper;
            set => _damper = value;
        }

        public float Target
        {
            get => _target;
            set => _target = value;
        }

        public float Velocity
        {
            get => _velocity;
            // Opcional: Si quieres un setter público, úsalo así.
            // Si no quieres que la velocidad se cambie manualmente desde fuera, ¡omite este setter!
            set => _velocity = value; 
        }

        public float Value
        {
            get => _value;
            // Es crucial tener un setter para Value, ya que permite 
            // establecer la posición inicial del resorte (como en la línea `_value = {set}` original).
            set => _value = value;
        }

        public void Update()
        {
            var delta = _target - _value;
            var direction = delta >= 0 ? 1f : -1f;
            var force = Mathf.Abs(delta) * _strength;
            _velocity += (force * direction - _velocity * _damper) * Time.deltaTime;
            _value += _velocity * Time.deltaTime;
        }

        public void Reset()
        {
            _velocity = 0f;
            _value = 0f;
        }
    }
}

