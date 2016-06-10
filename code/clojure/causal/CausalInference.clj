(use 'clojure.set)


; from http://stackoverflow.com/questions/8641305/find-index-of-an-element-matching-a-predicate-in-clojure
(defn indices [pred coll]
	(into [] (keep-indexed #(when (pred %2) %1) coll)))

; nodemetadata
; ============
; is an array for the metadata of the dag
; the values are integers where the information is bitwise stored
; bit index 0 : marked
; bit index 1 : is end
; bit index 2 : special marking

; nodes are folded until an isEnd node is hit
; folding is just the grouping of nodes into one node and create another level with an entry node, the nodes which got folded and an exit node
; this is possible and required to preserve invariants and accelerate the algorithm with an divide and conquer approach




(defn appendIfNotInThere [list toAppending]
	(into [] (union (into #{} list) (into #{} toAppending))))

; test  (translateOutLinksToOutIndicesAsList {:outLinks #{ {:target 1 :strength 1} {:target 3 :strength 1}} :inWeight 1 :outWeight 1 })
(defn translateOutLinksToOutIndicesAsList [dagEntry]
	(let [
		outLinksOfDagEntry (get dagEntry :outLinks)
		]
			(into [] (map (fn [link] (get link :target)) outLinksOfDagEntry))))


; modernized
(defn dagGetOutIndices [dag nodeIndex]
	(translateOutLinksToOutIndicesAsList (nth dag nodeIndex)))

; works
;(defn getSetOfAllFollowers [dag index]
;	(let [selectedDagElement (nth dag index)]
;		(get selectedDagElement :outIndices )))

; works
; test
; (getSetOfAllFollowers [{:outIndices #{1}} {:outIndices #{}}] 0)
(defn getSetOfAllFollowers [dag entryIndex]
	(do
		(println "getSetOfAllFollowers called with entryIndex " entryIndex)

		(let [followerIndices (dagGetOutIndices dag entryIndex)
          reduceFn (fn [a b] (union a (getSetOfAllFollowers dag b)))
          mapedFollowersAsSet  (into #{} (map (fn [a] (getSetOfAllFollowers dag a)) followerIndices))
         ]
			
         (reduce union followerIndices mapedFollowersAsSet))))






; works
; modernized
; test
; (markNodesIntern [{:outLinks #{ {:target 1 :strength 1}} :inWeight 1 :outWeight 1 }  {:outLinks #{} :inWeight 1 :outWeight 1 }] [0 0] [0])
(defn markNodesIntern [dag nodemetadata openlist]
	(if (== (count openlist) 0)
		nodemetadata
		(let [nodeindex (first openlist)
					openlist1 (rest openlist)

					typeOfNode (nth nodemetadata nodeindex)

					typeOfNodeIsMarked (== (bit-and 1 typeOfNode) 1)
					typeOfNodeIsEnd (== (bit-and 2 typeOfNode) 2)
					endreached (or typeOfNodeIsMarked typeOfNodeIsEnd)
					]
			(if endreached
				nodemetadata
				(let
					[newNodeMetadata (bit-or 1 typeOfNode)
					 nodemetadata1 (assoc nodemetadata nodeindex newNodeMetadata)
					 openlist2 (appendIfNotInThere openlist1 (dagGetOutIndices dag nodeindex))]
					(recur dag nodemetadata1 openlist2))))))




; DAG (in clojure) : 
; list of {:outIndices   <set of out indices>}

;- local DAG
;- local flags for local DAG elements

; marks this and all following nodes with bit 1 until it reaches bit 2 or no follower is found

(defn markNodes [dag nodemetadata entryIndex]
	(markNodesIntern dag nodemetadata [entryIndex]))

;(defn markFirstInOpenlist [dag nodemetadata openlist]
;	(let [nodemetadata (get :nodemetadata nodemetadata)]
;		{:nodemetadata (markFirstInOpenlistIntern dag nodemetadata openlist)}))




; getRandomFollower

(defn dagGetRandomFollowerIndex [dag entryIndex]
	(rand-nth (into [] (getSetOfAllFollowers dag entryIndex))))

; marks a random following node as 2 (marked as end)

(defn markRandomFollowerAsEnd [dag nodemetadata entryIndex]
	(let [
		followerIndexToMark (dagGetRandomFollowerIndex dag entryIndex)
		oldMetadataValue (nth nodemetadata followerIndexToMark)
		newMetadataValue (bit-or 2 oldMetadataValue)
		]
			{:nodemetadata (assoc nodemetadata followerIndexToMark newMetadataValue)}))


; seems to work
; (markNAsEnd [{:outIndices #{1}} {:outIndices #{}}] [0 0] 0 1)
(defn markNAsEnd [dag nodemetadata entryIndex counter]
	(if (== counter 0)
		nodemetadata

		(let [
			randomFollowerIndex (dagGetRandomFollowerIndex dag entryIndex)
			oldMetadataValue (nth nodemetadata randomFollowerIndex)
			newMetadataValue (bit-or oldMetadataValue 2)
			nodemetadata1 (assoc nodemetadata randomFollowerIndex newMetadataValue)]
				(recur dag nodemetadata1 entryIndex (- counter 1)))))











; modernized

; takes a list and returns a list
(defn dagHelperTranslateLinksAsList [outLinks mapping newCreatedIndex]
	(let [
		translateSingleLinkFn
			(fn [link]
				(let [
					firstRemainingOutLinkTarget (get link :target)
					firstRemainingOutLinkStrength (get link :strength)

					mappingForSelectedDagElement (get mapping firstRemainingOutLinkTarget)
					mappingForSelectedDagElementWasCollapsed (== 0 (compare :collapsed (get mappingForSelectedDagElement :renaming)))

					translatedOutIndex
						(if mappingForSelectedDagElementWasCollapsed
							newCreatedIndex
							(get mappingForSelectedDagElement :renamingIndex))

					translatedLink {:target translatedOutIndex :strength firstRemainingOutLinkStrength}
					]
						translatedLink))
				

		]
			(into [] (map translateSingleLinkFn outLinks))))






; modernized
; helper
; returns a list

(defn accumulateStrengthsOfLinksList [allOutLinksAsList]
	(let [
		rawNotTranslatedAccumulatedOutLinksAsDict
			(loop [remainingConnections allOutLinksAsList resultStrengthByTargetDict {}]
				(if (== 0 (count remainingConnections))
					resultStrengthByTargetDict
					(let [
						currentConnection (first remainingConnections)
						remainingConnections1 (rest remainingConnections)


						currentTargetIndex (get currentConnection :target)
						currentTargetStrength (get currentConnection :strength)

						resultStrengthByTargetDict1
							(if (contains? resultStrengthByTargetDict currentTargetIndex)
								; add strength to strength of dict
								(assoc resultStrengthByTargetDict currentTargetIndex (+ currentTargetStrength (get resultStrengthByTargetDict currentTargetIndex)))

								; create new dict entry and set strength
								(assoc resultStrengthByTargetDict currentTargetIndex currentTargetStrength))
						]
							(recur remainingConnections1 resultStrengthByTargetDict1))))


		; helper
		helperCreateLinkWithStrengthFromDictFn
			(fn [targetIndex]
				{:target targetIndex :strength (get rawNotTranslatedAccumulatedOutLinksAsDict targetIndex)})

		; translate the rawNotTranslatedAccumulatedOutLinksAsDict to outLinks dict
		notTranslatedOutlinks (map helperCreateLinkWithStrengthFromDictFn (keys rawNotTranslatedAccumulatedOutLinksAsDict))
		]
			notTranslatedOutlinks))





; needs a map which map  input node indices to a dict

; :renamingIndex is the calculated new index after renaming

; :renaming is either
;  :inGraph    the node stays in the graph
;  :collapsed  the node got collapsed to the new created node

; and the index of the new created dag element (where the index should be equal to the length of the resuling graph)

; doesn't reorder the result
; doesn't collapse the links but add the strength >


; returns the new dag with "renamed" connections

; modernized
; works
; test (remappingDag
;			[{:outLinks #{ {:target 1 :strength 1} {:target 2 :strength 1} } :inWeight 1 :outWeight 1 }  {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 }] 
;			{ 0 {:renaming :inGraph  :renamingIndex 0}    1 {:renaming :collapsed}     2 {:renaming :collapsed}    3 {:renaming :collapsed }     4 {:renaming :collapsed}   }
;			1
;	)

(defn remappingDag [dag mapping newCreatedIndex]
	(let [
		renameFn
			(fn [dagEntry]
				(let [
					outLinksOfNode (get dagEntry :outLinks)
					outLinksOfNodeAsList (into [] outLinksOfNode)

					renamedOutLinksOfNode (into #{} (accumulateStrengthsOfLinksList (dagHelperTranslateLinksAsList outLinksOfNodeAsList mapping newCreatedIndex)))
					]
						(assoc dagEntry :outLinks renamedOutLinksOfNode)))
		]
			(into [] (map renameFn dag))))



; helper
; returns if the dag element got collapsed
(defn dagHelperIsDagElementCollapsed [mapping indexOfDagElement]
	(let [
		mappingByIndex (get mapping indexOfDagElement)
		renamingForDagElement (get mappingByIndex :renaming)

		collapsed (== 0 (compare :collapsed renamingForDagElement))
		]
			collapsed))

; helper
(defn filterByCollapsed [dag mapping collapsedFn]
	(let [
		predicateFn
			(fn [dictWithDagElementAndIndex]
				(let [
					index1 (get dictWithDagElementAndIndex :index)
					collapsed (dagHelperIsDagElementCollapsed mapping index1)
					]
						(collapsedFn collapsed)))

		extractDagElementFn
			(fn [dict] (get dict :dagElement))

		zipFn
			(fn [dagElement index] {:index index :dagElement dagElement})

		indices (take (count dag) (iterate inc 0))
		]
			(map extractDagElementFn (filter predicateFn (map zipFn dag indices)))))

; remove all collapsed nodes

; modernized
; works
; test (dagCollapseByMapping
;			[{:outLinks #{ {:target 1 :strength 1} {:target 2 :strength 1} } :inWeight 1 :outWeight 1 }  {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 }] 
;			{ 0 {:renaming :inGraph  :renamingIndex 0}    1 {:renaming :collapsed}     2 {:renaming :collapsed}    3 {:renaming :collapsed }     4 {:renaming :collapsed}   }
;     )
(defn dagCollapseByMapping [dag mapping]
	(filterByCollapsed dag mapping (fn [collapsed] (not collapsed))))







; collapses all collapsed nodes into one node (which is returned) and calculates the new links and weights
; output links will be remapped
; needs a notremapped dag

; modernized
; works
; test
;(dagCalcCollapsedNode
;			[{:outLinks #{ {:target 1 :strength 1} {:target 2 :strength 1} } :inWeight 1 :outWeight 1 }  {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 }] 
;			{ 0 {:renaming :inGraph  :renamingIndex 0}    1 {:renaming :collapsed}     2 {:renaming :collapsed}    3 {:renaming :collapsed }     4 {:renaming :collapsed}   }
;			1
;     )
(defn dagCalcCollapsedNode [dag mapping newCreatedIndex]
	(let [
		rawNotTranslatedDagNodesWhichGotCollapsed (filterByCollapsed dag mapping (fn [collapsed] collapsed))

		; now we extract all outlinks as a list
		; we need it as a list because we need to add up strength to the same target
		; is a lazy list of sets
		rawNotTranslatedAllOutLinksOfDagNodes (map (fn [dagElement] (get dagElement :outLinks)) rawNotTranslatedDagNodesWhichGotCollapsed)
		
		; translate it to a lazy list of lists
		rawNotTranslatedAllOutLinksOfDagNodesAsLists (map (fn [set] (into [] set)) rawNotTranslatedAllOutLinksOfDagNodes)

		; concatenate lists
		rawNotTranslatedAllOutLinksOfDagNodesAsOneList (flatten rawNotTranslatedAllOutLinksOfDagNodesAsLists)

		; translate the rawNotTranslatedAccumulatedOutLinksAsDict to outLinks dict
		notTranslatedOutlinks (accumulateStrengthsOfLinksList rawNotTranslatedAllOutLinksOfDagNodesAsOneList)

		; translate outlinks
		renamedOutLinksOfNodeAsList (dagHelperTranslateLinksAsList notTranslatedOutlinks mapping newCreatedIndex)
		renamedOutLinksOfNodeAsSet (into {} renamedOutLinksOfNodeAsList)

		; accumulate inWeight and outweight
		accumulatedInWeights (reduce + (map (fn [dict] (get dict :inWeight)) rawNotTranslatedDagNodesWhichGotCollapsed))
		accumulatedOutWeights (reduce + (map (fn [dict] (get dict :outWeight)) rawNotTranslatedDagNodesWhichGotCollapsed))
		]
			{:outLinks renamedOutLinksOfNodeAsSet  :inWeight accumulatedInWeights  :outWeight accumulatedOutWeights}))





; modernized

; this is a essential function of the new causal inference algorithm
; ==================================================================

; function which
; * remaps the dag
; * collapses the result
; * adds the last dagEntry for the collapsed node

; test
;(dagCalcRemappedGraphWithCollapsedNode
;			[{:outLinks #{ {:target 1 :strength 1} {:target 2 :strength 1} } :inWeight 1 :outWeight 1 }  {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 }] 
;			{ 0 {:renaming :inGraph  :renamingIndex 0}    1 {:renaming :collapsed}     2 {:renaming :collapsed}    3 {:renaming :collapsed }     4 {:renaming :collapsed}   }
;     )
(defn dagCalcRemappedGraphWithCollapsedNode [dag mapping]
	(let [
		newCreatedIndex (count (filterByCollapsed dag mapping (fn [collapsed] (not collapsed))))

		colapsedDag (dagCollapseByMapping dag mapping)
		remappedColapsedDag (remappingDag colapsedDag mapping newCreatedIndex)

		colapsedNodeWithRemapping (dagCalcCollapsedNode dag mapping newCreatedIndex)
		]
			(concat remappedColapsedDag [colapsedNodeWithRemapping])))











; tested, works
(defn arrayExcept [array index]
	(let [
		lengthOfArray (count array)

		firstPart (subvec array 0 index)
		lastPart (subvec array (+ index 1) lengthOfArray)
		]
			(vec (concat firstPart lastPart))))


; classical old algorithm

; returns indices of elements in dag

; modernized
; works
; test (calcRandomOrderOfCausalSet [] [{:outLinks #{ {:target 1 :strength 1} {:target 2 :strength 1} } :inWeight 1 :outWeight 1 }  {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 }] [0])
(defn calcRandomOrderOfCausalSet [concatResult dag openListOutIndicesAsList]
	(if (== 0 (count openListOutIndicesAsList))
		concatResult

		; select one random index of openListOutIndicesAsList
		(let 
			[
			openListOutIndicesIndex (rand-int (count openListOutIndicesAsList))
			openListOutIndices1 (arrayExcept openListOutIndicesAsList openListOutIndicesIndex)
			
			currentDagIndex (nth openListOutIndicesAsList openListOutIndicesIndex)
			currentDagElement (nth dag currentDagIndex)
			currentDagElementOutIndices (translateOutLinksToOutIndicesAsList currentDagElement)

			openListOutIndices2 (into [] (union (into #{} openListOutIndices1) currentDagElementOutIndices))


			]
				(do
					(println "currentDagElementOutIndices " currentDagElementOutIndices)

				(recur (concat concatResult [currentDagIndex]) dag openListOutIndices2)))))





; calcs energy of reordering

; modernized
; works
; test (calcEnergyOfReordering [{:outLinks #{ {:target 1 :strength 1} {:target 2 :strength 1} } :inWeight 1 :outWeight 1 }  {:outLinks #{} :inWeight 1 :outWeight 1 } {:outLinks #{} :inWeight 1 :outWeight 1 }]  [0 1 2])
(defn calcEnergyOfReordering [dag reordering]
	(let [
		translateSingleIndexToNewOrdering
			(fn [indexToTranslate]
				(nth reordering indexToTranslate))

		translateOutLinksToNewOrderingFn
			(fn [outLinks]
				(let [
					translateSingleLink
						(fn [link]
							{:target (translateSingleIndexToNewOrdering (get link :target)) :strength (get link :strength)})
					]
						(into #{} (map translateSingleLink outLinks))))
			 


		; translates the indices of a dag entry to the new indices
		translateDagEntry
			(fn [dagEntry]
				{
					:outLinks (translateOutLinksToNewOrderingFn (get dagEntry :outLinks))
					:inWeight (get dagEntry :inWeight)
					:outWeight (get dagEntry :outWeight)
				})
		
		; calculate the energy of one dag element
		calcEnergyOfDagElementFn
			(fn [dagEntry dagIndex] 
				(let [
					calcEnergyOfLinkFn
						(fn [link]
							(let [ 
								targetIndex (get link :target)
								linkStrength (get link :strength)
								]
									(* linkStrength (- targetIndex dagIndex))))
					]
						(reduce + (map calcEnergyOfLinkFn (into [] (get dagEntry :outLinks))))))

		translatedDag (map translateDagEntry dag)

		; generates all indices for the dag
		indices (take (count dag) (iterate inc 0))

		energiesOfDagElements (reduce + (map calcEnergyOfDagElementFn translatedDag indices))
		]
			energiesOfDagElements))








; helper
(defn markRandomFollowerAsEndAndfill [dag nodemetadata entryIndex]
	(let [
		  endMarkedNodeMetadataAssoc (markRandomFollowerAsEnd dag nodemetadata entryIndex)
		  endMarkedNodeMetadata (get endMarkedNodeMetadataAssoc :nodemetadata)
		  result (markNodes dag endMarkedNodeMetadata entryIndex)
		  ]
		result))

; helper
(defn checkMarkingAndSpecialMarkingOverlap [nodemetadata]
	(complement (not-any? #(== (bit-and 5 %) 5) nodemetadata)))


; helper
; tries to find a nonoverlapping marking of a group of nodes
; a overlapping is the case if bit 2 is set and bit 0 is set for the same node

; returns
; {
;  :foundNonoverlapping <bool>
;  :nodemetadata
; }
(defn tryFindNonoverlappingMarking [dag nodemetadata numberOfTries]
	(let [currentIteration 0]
		(if (> currentIteration numberOfTries)
			{:foundNonoverlapping false}
			(let [entryIndex (rand-int (count dag))
				  proposedNodeMetadata (markRandomFollowerAsEndAndfill dag nodemetadata entryIndex)
				  overlapWithSpecialMarking (checkMarkingAndSpecialMarkingOverlap proposedNodeMetadata)]
				(if overlapWithSpecialMarking
					(recur (+ 1 currentIteration))
					{:foundNonoverlapping true :nodemetadata proposedNodeMetadata})))))

; helper for "dagTryToCreateNClusters"
; returns list of indices of nodes where the nodemetadata is true with the mask (for all bits in the mask)
(defn extractNodeIndicesOfNodesMarkedWith [nodemetadata mask]
	(indices #(== (bit-and mask %) mask) nodemetadata))

; helper for "dagTryToCreateNClusters"
(defn createEmptyMetadataForDag [dag]
	(repeat (count dag) 0))




; helper function for "dagRecursivlyFindBestReordering"

; returns cluster information in the form of a list of
; { :nodeIndices <indices of input dag nodes which are in the cluster>
; }

(defn dagTryToCreateNClusters [dag numberOfClusteringsToTry numberOfOverlappingTries]
	(loop [currentFoldedDag dag
		   currentNodeMetadata (createEmptyMetadataForDag dag)
		   resultclusterInformation []
		   clusterI 0
		   ]

		; try to find a mapping which doesn't fold the already folded nodes
		(let [tryFindNonoverlappingMarkingAssoc (tryFindNonoverlappingMarking dag currentNodeMetadata numberOfOverlappingTries)
			  tryFindNonoverlappingMarkingWasSuccessful (get tryFindNonoverlappingMarkingAssoc :foundNonoverlapping)]
			(if (not tryFindNonoverlappingMarkingWasSuccessful)
				resultclusterInformation; return

				(let [newNonoverlappingNodeMetadata (get tryFindNonoverlappingMarkingAssoc :nodemetadata)
					  indicesOfMarkedNodes (extractNodeIndicesOfNodesMarkedWith newNonoverlappingNodeMetadata 1)

					  ; set mask to 4 for the indicesOfMarkedNodes to get the new mask
					  ; TODO TODO TODO

					  ])


				)




		; TODO< store remapping for tracing and returning, so we can reconstruct the nodes later >

		)

	)








; new causal inference main algorithm
; combines all of the subalgorithms above

; we repeat for each level until there are below "noClusteringLimit" nodes left
; (A)
; * split it into n clusters (where n depens on )
; * do the whole process recursivly and return the best reordering

; optimize the energy of (A) with the permutation algorithm

; 
; we do (A) as long as the energy didn't converge or a iterationlimit is reached


; parameters:
; "noClusteringLimit" how many nodes does the network contain that clusterings are tried?  (set this to 0 to always cluster to test the clustering algorithm)
; "permutationMaxLoops" how many times should a permutation be tried for the nonclustering case (simple combinatorial algorithm)
; "maxNumberOfClusteringsToTry" how many maximal clusterings should be tried

(defn dagRecursivlyFindBestReordering [dag  noClusteringLimit  permutationMaxLoops  maxNumberOfClusteringsToTry]
	(let [
		numberOfElementsInDag (count dag)
		numberOfElementsInDagBelowClusteringLimit (< numberOfElementsInDag noClusteringLimit)

		; function which gets execute if the number of the elements in dag are below the cluster limit
		localReorderingFn
			(fn [] 
				(loop [
					currentIterationNumber 0
					bestOrder nil
					bestEnergy Double/POSITIVE_INFINITY
					]
					(if (>= currentIterationNumber permutationMaxLoops)
						{:minEnergy bestEnergy :reordering bestOrder}

						(let [
							listOfEntryDagIndices (TODO CALLFUNCTION TO CALCULATE THIS)
							currentReodering (calcRandomOrderOfCausalSet [] dag listOfEntryDagIndices)
							currentEnergyOfReodering (calcEnergyOfReordering dag currentReodering)
							]
							(if (< currentEnergyOfReodering bestEnergy)
								(recur (+ 1 currentIterationNumber) currentReodering currentEnergyOfReodering)
								(recur (+ 1 currentIterationNumber) bestOrder bestEnergy))))))

		; function which gets executed if the number of the elements in the dag are above the cluster limit
		clusterFn
			(fn []
				(let [
					numberOfClusteringsToTry maxNumberOfClusteringsToTry ; we set it to the maximal value because we don't have jet any heuristic to choose it based on the nodecount

					; do the number of tries to create clusters
					clusterInformation (dagTryToCreateNClusters dag numberOfClusteringsToTry)

					]
						; TODO< optimize each cluster by calling this function "dagRecursivlyFindBestReordering" recursivly >

						; TODO< do reodering algorithm >

						; TODO< unpack result ordering >

					)
				)

		]
			(if numberOfElementsInDagBelowClusteringLimit
				(localReorderingFn)
				(clusterFn))))


	

; {:outLinks #{ {:target 1 :strength 1} } :inWeight 1 :outWeight 1 } }