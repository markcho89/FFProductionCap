# FFProductionCap
Farthest Frontier Production Cap Mod

You can set the production cap of buildings. If the unused amount of an item is over the limit, all buildings producing the item will be disabled (freeing up the workers,too).
If the item is used up and unused amount is below the limit, the buildings will be enabled again preserving the worker count.

To set the production cap, modify the ProductionCap.xml file, set StopAt. If you do not want the cap, simply remove the xml node or set it to large number (2147483647 is max).
You can set the ResumeAt so that the building can remain inactive until the item count drops below ResumeAt.
The production cap is loaded on game launch and it cannot be changed mid-game (changes coming).

Buildings not supported:
<ul>
  <li>All Housing buildings</li>
  <li>All Amenity buildings except Apothecary Shop</li>
  <li>All Storage buildings except Cooper</li>
  <li>Farms</li>
  <li>Barns</li>
  <li>Hunter cabin</li>
  <li>Forager shack</li>
  <li>Arborist building</li>
  <li>Perservist building</li>
  <li>Well</li>
  <li>Work Camp</li>
  <li>Apiary</li>
  <li>All Defence buildings</li>
</ul>

Known Issues:
<ul>
  <li>When buildings with production slider resumes production, the production ratio will resume at 1. The mod will not resume at the ratio it used to have. If you wish to modify the ratio, you need to manually select the building and modify the ratio slider.</li>
</ul>

Upcoming changes:
<ul>
  <li>Add slider on resource UI to set the production cap in game, other than on game launch
</ul>
