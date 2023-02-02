namespace CaptainHindsight
{
    public interface IInteractable
    {
        public void ChangeGravity(float newGravity);

        public void ResetGravity();

        public void ClampSpeed(float maxSpeed);

        public void OneWayMovement(bool leftForbidden, bool rightForbidden);
    }
}