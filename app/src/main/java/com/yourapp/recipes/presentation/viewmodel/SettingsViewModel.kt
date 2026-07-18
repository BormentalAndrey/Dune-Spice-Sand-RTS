package com.yourapp.recipes.presentation.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.yourapp.recipes.domain.model.UserPreferences
import com.yourapp.recipes.domain.repository.RecipeRepository
import com.yourapp.recipes.domain.repository.UserPreferencesRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class SettingsViewModel @Inject constructor(
    private val userPreferencesRepository: UserPreferencesRepository,
    private val recipeRepository: RecipeRepository
) : ViewModel() {
    
    val preferences = userPreferencesRepository.getPreferences()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5000), UserPreferences())
    
    private val _recipeCount = MutableStateFlow(0)
    val recipeCount: StateFlow<Int> = _recipeCount.asStateFlow()
    
    private val _isExporting = MutableStateFlow(false)
    val isExporting: StateFlow<Boolean> = _isExporting.asStateFlow()
    
    private val _isImporting = MutableStateFlow(false)
    val isImporting: StateFlow<Boolean> = _isImporting.asStateFlow()
    
    init {
        loadRecipeCount()
    }
    
    fun updateTheme(themeMode: String) {
        viewModelScope.launch {
            userPreferencesRepository.updateThemeMode(themeMode)
        }
    }
    
    fun updateMeasurementSystem(system: String) {
        viewModelScope.launch {
            userPreferencesRepository.updateMeasurementSystem(system)
        }
    }
    
    fun savePreferences(preferences: UserPreferences) {
        viewModelScope.launch {
            userPreferencesRepository.savePreferences(preferences)
        }
    }
    
    private fun loadRecipeCount() {
        viewModelScope.launch {
            _recipeCount.value = recipeRepository.getRecipeCount()
        }
    }
    
    fun exportRecipes() {
        viewModelScope.launch {
            _isExporting.value = true
            try {
                // Export logic will be implemented
            } finally {
                _isExporting.value = false
            }
        }
    }
    
    fun importRecipes(jsonData: String) {
        viewModelScope.launch {
            _isImporting.value = true
            try {
                // Import logic will be implemented
            } finally {
                _isImporting.value = false
            }
        }
    }
}
