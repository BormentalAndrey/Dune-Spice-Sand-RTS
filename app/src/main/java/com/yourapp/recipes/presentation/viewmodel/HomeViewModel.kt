package com.yourapp.recipes.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.yourapp.recipes.domain.model.*
import com.yourapp.recipes.domain.usecase.recipe.*
import com.yourapp.recipes.domain.repository.RecipeRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class HomeViewModel @Inject constructor(
    private val getFilteredRecipesUseCase: GetFilteredRecipesUseCase,
    private val toggleFavoriteUseCase: ToggleFavoriteUseCase,
    private val recipeRepository: RecipeRepository
) : ViewModel() {
    
    private val _filter = MutableStateFlow(RecipeFilter())
    val filter: StateFlow<RecipeFilter> = _filter.asStateFlow()
    
    private val _recipes = _filter.flatMapLatest { filter ->
        getFilteredRecipesUseCase(filter)
    }.stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), emptyList())
    
    val recipes: StateFlow<List<Recipe>> = _recipes
    
    private val _randomRecipe = MutableStateFlow<Recipe?>(null)
    val randomRecipe: StateFlow<Recipe?> = _randomRecipe.asStateFlow()
    
    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()
    
    init {
        loadRandomRecipe()
    }
    
    fun updateFilter(newFilter: RecipeFilter) {
        _filter.value = newFilter
    }
    
    fun updateSearchQuery(query: String) {
        _filter.update { it.copy(searchQuery = query) }
    }
    
    fun updateCategory(category: Category?) {
        _filter.update { it.copy(category = category) }
    }
    
    fun updateDifficulty(difficulty: Difficulty?) {
        _filter.update { it.copy(difficulty = difficulty) }
    }
    
    fun updateMaxCookingTime(minutes: Int?) {
        _filter.update { it.copy(maxCookingTime = minutes) }
    }
    
    fun toggleFavorites() {
        _filter.update { it.copy(onlyFavorites = !it.onlyFavorites) }
    }
    
    fun updateSortBy(sortBy: SortOption) {
        _filter.update { it.copy(sortBy = sortBy) }
    }
    
    fun toggleFavorite(recipeId: Long, isFavorite: Boolean) {
        viewModelScope.launch {
            toggleFavoriteUseCase(recipeId, isFavorite)
        }
    }
    
    fun loadRandomRecipe() {
        viewModelScope.launch {
            _isLoading.value = true
            try {
                _randomRecipe.value = recipeRepository.getRandomRecipe()
            } catch (e: Exception) {
                // Handle error
            } finally {
                _isLoading.value = false
            }
        }
    }
    
    fun deleteRecipe(recipeId: Long) {
        viewModelScope.launch {
            recipeRepository.deleteRecipe(recipeId)
        }
    }
}
