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
    
    val recipes: StateFlow<List<Recipe>> = _filter
        .flatMapLatest { filter -> getFilteredRecipesUseCase(filter) }
        .stateIn(
            scope = viewModelScope,
            started = SharingStarted.WhileSubscribed(5000),
            initialValue = emptyList()
        )
    
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
        _filter.value = _filter.value.copy(searchQuery = query)
    }
    
    fun updateCategory(category: Category?) {
        _filter.value = _filter.value.copy(category = category)
    }
    
    fun updateDifficulty(difficulty: Difficulty?) {
        _filter.value = _filter.value.copy(difficulty = difficulty)
    }
    
    fun updateMaxCookingTime(minutes: Int?) {
        _filter.value = _filter.value.copy(maxCookingTime = minutes)
    }
    
    fun toggleFavorites() {
        _filter.value = _filter.value.copy(onlyFavorites = !_filter.value.onlyFavorites)
    }
    
    fun updateSortBy(sortBy: SortOption) {
        _filter.value = _filter.value.copy(sortBy = sortBy)
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
                e.printStackTrace()
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
