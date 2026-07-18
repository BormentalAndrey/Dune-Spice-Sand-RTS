package com.yourapp.recipes.domain.usecase.recipe

import com.yourapp.recipes.domain.model.Recipe
import com.yourapp.recipes.domain.model.RecipeFilter
import com.yourapp.recipes.domain.repository.RecipeRepository
import kotlinx.coroutines.flow.Flow
import javax.inject.Inject

class GetFilteredRecipesUseCase @Inject constructor(
    private val recipeRepository: RecipeRepository
) {
    operator fun invoke(filter: RecipeFilter): Flow<List<Recipe>> {
        return recipeRepository.getFilteredRecipes(filter)
    }
}
