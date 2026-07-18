package com.yourapp.recipes.presentation.viewmodel

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.yourapp.recipes.domain.model.*
import com.yourapp.recipes.domain.repository.RecipeRepository
import com.yourapp.recipes.domain.repository.ShoppingListRepository
import com.yourapp.recipes.domain.usecase.recipe.CalculateNutritionUseCase
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class RecipeDetailViewModel @Inject constructor(
    savedStateHandle: SavedStateHandle,
    private val recipeRepository: RecipeRepository,
    private val shoppingListRepository: ShoppingListRepository,
    private val calculateNutritionUseCase: CalculateNutritionUseCase
) : ViewModel() {
    
    private val recipeId: Long = savedStateHandle.get<Long>("recipeId") ?: 0L
    
    private val _recipe = recipeRepository.getRecipeByIdFlow(recipeId)
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), null)
    
    val recipe: StateFlow<Recipe?> = _recipe
    
    private val _servingsMultiplier = MutableStateFlow(1)
    val servingsMultiplier: StateFlow<Int> = _servingsMultiplier.asStateFlow()
    
    private val _adjustedNutrition = combine(_recipe, _servingsMultiplier) { recipe, multiplier ->
        recipe?.let {
            calculateNutritionUseCase(it.nutritionInfo, multiplier.toFloat())
        }
    }.stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), null)
    
    val adjustedNutrition: StateFlow<NutritionInfo?> = _adjustedNutrition
    
    private val _completedSteps = MutableStateFlow<Set<Int>>(emptySet())
    val completedSteps: StateFlow<Set<Int>> = _completedSteps.asStateFlow()
    
    val cookingProgress: StateFlow<Float> = combine(_recipe, _completedSteps) { recipe, completed ->
        if (recipe?.steps?.isEmpty() == true) 0f
        else completed.size.toFloat() / (recipe?.steps?.size ?: 1)
    }.stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), 0f)
    
    fun updateServingsMultiplier(multiplier: Int) {
        _servingsMultiplier.value = multiplier
    }
    
    fun toggleStepCompletion(stepNumber: Int) {
        _completedSteps.update { current ->
            if (current.contains(stepNumber)) {
                current - stepNumber
            } else {
                current + stepNumber
            }
        }
    }
    
    fun addToShoppingList(servings: Int = 1) {
        viewModelScope.launch {
            _recipe.value?.let { recipe ->
                shoppingListRepository.addIngredientsToShoppingList(
                    recipe.ingredients,
                    servings,
                    recipe.id
                )
            }
        }
    }
    
    fun toggleFavorite() {
        viewModelScope.launch {
            _recipe.value?.let { recipe ->
                recipeRepository.updateFavoriteStatus(recipe.id, !recipe.isFavorite)
            }
        }
    }
    
    fun saveRecipe(updatedRecipe: Recipe) {
        viewModelScope.launch {
            recipeRepository.saveRecipe(updatedRecipe)
        }
    }
}
