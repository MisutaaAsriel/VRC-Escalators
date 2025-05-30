# VRC Escalators
#### Version 1.0.1
### A simple package to easily add seamless escalation to... escalators
###### What, did you want an elevator? *Get outta 'ere! ...though. 🤔*

## How to Install (UPM)
1. [Add this repo to the Unity Package Manager](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-ui-giturl.html) using the following URL: `https://github.com/MisutaaAsriel/VRC-Escalators.git`

## Manual Installation (VPM)
1. [Download this repo as a ZIP file](https://github.com/MisutaaAsriel/VRC-Escalators/archive/refs/heads/main.zip) and unzip it into a folder.
2. Open the VCC or your VCC alternative of choice, and [add the unzipped folder as a User Package](https://vcc.docs.vrchat.com/vpm/packages/#user-packages)

## How to Use
1. Select the `GameObject` menu, and choose `VRChat/Escalator` to add a preconfigured gameobject for controlling escallation.
2. Make the newly added `Escalator Utility` a child of the game object containing your escalator's mesh.
3. Move the `Escalator Utility` to the **center** of its parent.
4. Move the `Base` to the **tip of the base of your escalator or lower start point,** centering it with the area of travel.
5. Move the `Summit` to the **tip of the end of your escalator or upper start point,** centering it with the area of travel.
6. Resize the included `Box Collider` to **encapsulate the entire area of escalation, minus `0.15m` on all sides.¹**
7. Set the `Direction` of the `VRC Escalator` on the `Escalator Utility` to your desired direction.
8. Set the `Axis` on the `VRC Escalator` to **the *local* axis that is perpendicular to the direction of travel**.²
9. Configure your speed, update, and mobility options, as needed.
10. Weeeeeeeeeeeeeee~

> [!IMPORTANT]
> ¹ By proving a buffer of **at least** 0.15m **in global units** (i.e at a scale of 1.0 in world space), this prevents a few issues:
> 1. The player becomes stuck at the end of the escalator as they have not exited the trigger collider. This script uses `OnPlayerTriggerEnter` & `OnPlayerTriggerExit` to start and end movement, so the exit points need be outside the collider by some amount. 0.15m in testing seems to be an optimal number, but your mileage may vary.
> 2. Escelators have side rails, which you may wish for players to climb. If the collider is too close to the sides, a player's capsule may intersect, causing *unwanted* escalation.

> [!NOTE]
> ² A nicety of the escalator script is that it aligns the exit point with the player position, that way you go straight up or down, rather than "drifting" towards the center. This does so by inferring the player's position *in the escalator's local coordinate space*, and aligning the exit *on the specified axis*.
> * Why local coordinates? This allows the escalator to be rotated to an arbitrary degree not named `90º`, `180º`, `270º`, or `360º`. :P

## Advanced Usage
1. Add a `Collider` of your desired type and set it to `Trigger`, on an empty object as a child of your target object.³⁴
2. Add a `VRC Escalator` to this empty object and give the object an identifiable name
3. Add an empty object as a child of your escalator controller, and move it to the base of player movement, centered to the field of travel.
4. Select the escalator controller, and drag this new child object to the `Lower Level Position` slot of the `VRC Escalator`.
5. Add a second empty object as a child of your escalator controller, and move it to the upper end of player movement, centered to the field of travel.
6. Select the escalator controller, and drag this new child object to the `Upper Level Position` slot of the `VRC Escalator`.
7. Adjust your collider and positions as seen fit, then follow steps `7 - 9` from [How to Use](#how-to-use) above.

> [!IMPORTANT]
> ³ As explained in a previous section, this system uses `OnPlayerTriggerEnter` & `OnPlayerTriggerExit`, in conjunction with [`IsPlayerGrounded`](https://creators.vrchat.com/worlds/udon/players/player-positions#isplayergrounded) to operate. As such, it is best you choose a collider and size it in such a way that the player is guaranteed to be touching the ground whilst intersecting with this collider, **whilst not extending into the general areas around the escalator or field of travel**, as noted previously.

> [!NOTE]
> ⁴ Whilst you can place the script & collider directly on the target object, this will make adjusting the collider much more limited in fashion. As such, it is recommended to be on an object that is a child of the target, rather than upon the target object itself. Additionally, the object containing the script optimally needs to be a child of the object containing the mesh, for exit alignment to function correctly in all orientations. If you plan to use this script without a mesh or parent object, you may ignore this and carry on. :)

## Example Scene Preview
<img width="910" alt="image" src="https://github.com/user-attachments/assets/b535ab62-4d6f-432e-af8b-3966b1198355" />

### [In-Game Demo 🔗](https://vrchat.com/home/world/wrld_2990f384-59de-4ae6-ba6a-76d3d7293472/info)
